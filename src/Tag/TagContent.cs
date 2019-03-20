using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;

namespace Tag
{
    /// <summary>
    /// Represents the contents of a HTML/XML tag.
    /// Use T.g(...) to create one.
    /// </summary>
    public class TagContent
    {
        /// <summary>
        /// Name of the tag. If null, only the contents are rendered (a plain text section)
        /// </summary>
        [CanBeNull]public string Tag { get; set; }

        /// <summary>
        /// Contents of the tag, including all sub-tags
        /// </summary>
        public List<TagContent> Contents { get; set; }

        /// <summary>
        /// Raw text content of this tag, used only if `Contents` is empty
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// If true, the tag is rendered as an empty tag, and content are not rendered even if they are supplied.
        /// </summary>
        public bool IsEmpty { get; set; }

        /// <summary>
        /// Rendered property names and values
        /// </summary>
        public string Properties { get; set; }

        /// <summary>
        /// Render the contents of this tag, including sub-tags, excluding the tags themselves
        /// </summary>
        /// <param name="tagsToExclude">list of tag names that should be skipped when rendering. If null or empty, all tags will be rendered</param>
        [NotNull]public string ToPlainText(params string[] tagsToExclude)
        {
            using (var sb = new StringWriter(new StringBuilder(4096)))
            {
                StreamTo(sb, false, tagsToExclude);
                return sb.ToString() ?? "";
            }
        }

        /// <summary>
        /// Render this tag and its contents as a HTML/XML string
        /// </summary>
        [NotNull]public override string ToString()
        {
            using (var sb = new StringWriter(new StringBuilder(4096)))
            {
                StreamTo(sb);
                return sb.ToString() ?? "";
            }
        }

        /// <summary>
        /// Stream to a text writer.
        /// </summary><remarks>Saves some string generation over multiple 'ToString' calls?</remarks>
        public void StreamTo(TextWriter tw)
        {
            StreamTo(tw, true);
        }

        /// <summary>
        /// Stream to a text writer.
        /// </summary>
        /// <param name="tw">target text writer</param>
        /// <param name="renderTags">if true, the XML/HTML tags will be rendered. If false, plain text contents will be rendered</param>
        /// <param name="tagsToExclude">list of tag names that should be skipped when rendering. If null or empty, all tags will be rendered</param>
        public void StreamTo(TextWriter tw, bool renderTags, params string[] tagsToExclude)
        {
            if (tw == null) return;

            if (tagsToExclude != null && tagsToExclude.Length > 0 && tagsToExclude.Contains(Tag)) {
                return;
            }

            if (renderTags && Tag != null)
            {
                tw.Write('<');
                tw.Write(Tag);
                if (Properties != null) tw.Write(Properties);

                if (IsEmpty)
                {
                    tw.Write("/>");
                    return;
                }

                tw.Write('>');
            }
            if (Contents != null)
            {
                foreach (var tag in Contents)
                {
                    tag.StreamTo(tw, renderTags, tagsToExclude);
                }
            }
            else if (Text != null)
            {
                tw.Write(Text);
            }

            if (renderTags && Tag != null)
            {
                tw.Write("</");
                tw.Write(Tag);
                tw.Write('>');
            }
        }

        /// <summary>
        /// Stream to a byte stream with a given encoding.
        /// </summary>
        public void StreamTo(Stream outp, Encoding textEncoding)
        {
            if (outp == null) return;
            using (var tw = new StreamWriter(outp, textEncoding, 4096, true))
            {
                StreamTo(tw);
                tw.Flush();
            }
        }

        /// <summary>
        /// Encode as a byte array. Note: If you pass `Encoding.UTF8`, you will get a BOM at the start of your array. Use `new UTF8Encoding()` to avoid this.
        /// </summary>
        [NotNull]public byte[] ToBytes(Encoding encoding)
        {
            var ms = new MemoryStream(4096);
            StreamTo(ms, encoding);
            ms.Seek(0, SeekOrigin.Begin);
            return ms.ToArray() ?? new byte[0];
        }

        /// <summary>
        /// Encode as a byte array, containing a null character at the end.
        /// Note: If you pass `Encoding.UTF8`, you will get a BOM at the start of your array. Use `new UTF8Encoding()` to avoid this.
        /// For multi-byte encodings, this outputs zero-valued bytes of a length equivalent to a 'space' character.
        /// </summary>
        [NotNull]public byte[] ToNullTerminatedBytes(Encoding encoding)
        {
            var ms = new MemoryStream(4096);
            StreamTo(ms, encoding);
            var charBytes = encoding?.GetByteCount(" ") ?? 1;
            for (int i = 0; i < charBytes; i++) { ms.WriteByte(0); }
            ms.Seek(0, SeekOrigin.Begin);
            return ms.ToArray() ?? new byte[0];
        }

        /// <summary>
        /// Mark this as an empty tag. Will render as &lt;Tag/&gt; instead of &lt;Tag&gt;&lt;/Tag&gt;
        /// </summary>
        [NotNull]public TagContent Empty()
        {
            IsEmpty = true;
            return this;
        }

        /// <summary>
        /// Implicitly render to a string
        /// </summary>
        [NotNull]public static implicit operator string(TagContent t) => t?.ToString() ?? "";

        /// <summary>
        /// Implicitly convert a string to a content-only tag
        /// </summary>
        [NotNull]public static implicit operator TagContent(string s) {
            return T.g()[s];
        }

        /// <summary>
        /// Supply the contents of the tag. These will not be rendered if `IsEmpty` is true.
        /// Additional tags will be added after existing ones.
        /// </summary>
        [NotNull]public TagContent Add(params TagContent[] content)
        {
            Add((IEnumerable<TagContent>)content);
            return this;
        }

        /// <summary>
        /// Supply the contents of the tag. These will not be rendered if `IsEmpty` is true.
        /// Additional tags will be added after existing ones.
        /// </summary>
        [NotNull]public TagContent Add(IEnumerable<TagContent> content)
        {
            if (content == null) return this;

            IsEmpty = false;
            if (Contents == null) Contents = new List<TagContent>();

            if (Text != null) {
                Contents.Add(T.g()[Text]);
                Text = null;
            }
            Contents.AddRange(content);

            return this;
        }

        /// <summary>
        /// Supply the contents of the tag. These will not be rendered if `IsEmpty` is true.
        /// Additional tags will be added after existing ones.
        /// </summary>
        [NotNull]public TagContent this[params TagContent[] content]
        {
            get { return Add(content); }
        }
        
        /// <summary>
        /// Supply the text contents of the tag. These will not be rendered if `IsEmpty` is true or if child tags are added.
        /// </summary>
        [NotNull]public TagContent this[string content]
        {
            get
            {
                Text = content;
                return this;
            }
        }

        /// <summary>
        /// Reset the properties string using an array alternating between key and value
        /// </summary>
        public void SerialiseProperties(string[] properties)
        {
            if (properties == null) return;

            var limit = properties.Length - (properties.Length % 2);
            if (limit <= 0) return;

            var sb = new StringBuilder();
            for (int i = 0; i < limit; i += 2)
            {
                sb.Append(' ');
                sb.Append(properties[i]);
                sb.Append("=\"");
                sb.Append(properties[i + 1]);
                sb.Append("\"");
            }
            Properties = sb.ToString();
        }

        /// <summary>
        /// Create a duplicate tag, recursively cloning any contents
        /// </summary>
        [NotNull]public TagContent Clone() {
            var dup = new TagContent{
                Properties = Properties,
                IsEmpty = IsEmpty,
                Tag = Tag,
                Text = Text
            };
            if (Contents != null && Contents.Any()) {
                dup.Contents = Contents.Select(c=>c?.Clone()).ToList();
            }
            return dup;
        }

        /// <summary>
        /// Create a duplicate outer tag without content
        /// </summary>
        [NotNull]public TagContent EmptyClone() {
            return new TagContent{
                Properties = Properties,
                IsEmpty = IsEmpty,
                Tag = Tag
            };
        }


        /// <summary>
        /// Create duplicates of this tag, each with an extra property and child tags.
        /// The supplied type should be strings, TagContent, or tuples of (string, string, TagContent)
        /// </summary>
        /// <remarks>The weird reflection is to make Tuples work with multiple .Net framworks and versions</remarks>
        [NotNull]public TagContent Repeat<Tt>(params Tt[] content)
        {
            if (typeof(Tt) == typeof(string)) {
                return RepeatDirect(content as string[]);
            }
            if (typeof(Tt) == typeof(TagContent)) {
                return RepeatDirect(content as TagContent[]);
            }
            var container = T.g();
            if (content == null) return container;

            var item1 = typeof(Tt).GetRuntimeField("Item1");
            var item2 = typeof(Tt).GetRuntimeField("Item2");
            var item3 = typeof(Tt).GetRuntimeField("Item3");
            if (item1 != null && item2 != null && item3 != null)
            {
                foreach (var tag in content)
                {
                    var newTag = EmptyClone();
                    newTag.Properties += " " + item1.GetValue(tag) + "=\"" + item2.GetValue(tag) + "\"";
                    var val = item3.GetValue(tag);
                    newTag.Add((val as TagContent) ?? (val as string));
                    container.Add(newTag);
                }
            }
            return container;
        }

        [NotNull]private TagContent RepeatDirect(string[] content)
        {
            var container = T.g();
            if (content == null) return container;

            foreach (var tag in content)
            {
                container.Add( EmptyClone().Add(tag) );
            }
            return container;
        }
        
        [NotNull]private TagContent RepeatDirect(TagContent[] content)
        {
            var container = T.g();
            if (content == null) return container;

            foreach (var tag in content)
            {
                container.Add( EmptyClone().Add(tag) );
            }
            return container;
        }
    }
}