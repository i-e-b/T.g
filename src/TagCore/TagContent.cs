using System.Collections.Generic;
using System.IO;
using System.Text;

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
        public string Tag { get; set; }

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
        /// Render this tag and its contents as a HTML/XML string
        /// </summary>
        public override string ToString()
        {
            using (var sb = new StringWriter(new StringBuilder(4096)))
            {
                StreamTo(sb);
                return sb.ToString();
            }
        }

        /// <summary>
        /// Stream to a text writer.
        /// </summary><remarks>Saves some string generation over multiple 'ToString' calls?</remarks>
        public void StreamTo(TextWriter tw)
        {
            if (Tag != null)
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
                    tw.Write(tag);
                }
            }
            else if (Text != null)
            {
                tw.Write(Text);
            }

            if (Tag != null)
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
            using (var tw = new StreamWriter(outp, textEncoding, 4096, true))
            {
                StreamTo(tw);
                tw.Flush();
            }
        }


        /// <summary>
        /// Mark this as an empty tag. Will render as &lt;Tag/&gt; instead of &lt;Tag&gt;&lt;/Tag&gt;
        /// </summary>
        public TagContent Empty()
        {
            IsEmpty = true;
            return this;
        }

        /// <summary>
        /// Implicitly render to a string
        /// </summary>
        public static implicit operator string(TagContent t) => t.ToString();

        /// <summary>
        /// Implicitly convert a string to a content-only tag
        /// </summary>
        public static implicit operator TagContent(string s) {
            return T.g()[s];
        }

        /// <summary>
        /// Supply the contents of the tag. These will not be rendered if `IsEmpty` is true.
        /// Additional tags will be added after existing ones.
        /// </summary>
        public TagContent Add(params TagContent[] content)
        {
            Add((IEnumerable<TagContent>)content);
            return this;
        }

        /// <summary>
        /// Supply the contents of the tag. These will not be rendered if `IsEmpty` is true.
        /// Additional tags will be added after existing ones.
        /// </summary>
        public TagContent Add(IEnumerable<TagContent> content)
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
        public TagContent this[params TagContent[] content]
        {
            get { return Add(content); }
        }
        
        /// <summary>
        /// Supply the text contents of the tag. These will not be rendered if `IsEmpty` is true or if child tags are added.
        /// </summary>
        public TagContent this[string content]
        {
            get
            {
                Text = content;
                return this;
            }
        }

        /// <summary>
        /// Attempt to load a file as the tag's text content.
        /// </summary>
        public TagContent LoadFile(string filePath)
        {
            Text = File.ReadAllText(filePath); // could try storing the file and streaming out at render time?
            return this;
        }

        /// <summary>
        /// Reset the properties string using an array alternating between key and value
        /// </summary>
        public void SerialiseProperties(string[] properties)
        {
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
    }
}
