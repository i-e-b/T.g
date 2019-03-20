using System;
using JetBrains.Annotations;

namespace Tag
{
    /// <summary>
    /// HTML/XML Tag creator
    /// </summary>
    public static class T
    {
#pragma warning disable IDE1006 // Naming Styles
// ReSharper disable InconsistentNaming

        /// <summary>
        /// Create a new tag.
        /// <para>Example: <code>T.g("div", "class", "plain")</code> would give <code>&lt;div class="plain"&gt;&lt;/div&gt;</code> </para>
        /// </summary>
        /// <param name="tagName">Name of the tag</param>
        /// <param name="properties">list alternating between property name and value</param>
        [NotNull]public static TagContent g(string tagName, params string[] properties)
        {
            if (tagName == null) tagName = "";
            var empty = tagName.EndsWith("/", StringComparison.Ordinal);
            var t = new TagContent
            {
                Tag = empty ? tagName.TrimEnd('/') : tagName,
                IsEmpty = empty,
                Contents = null
            };

            t.SerialiseProperties(properties);

            return t;
        }

        /// <summary>
        /// Create a new tag and mark it as empty
        /// </summary>
        [NotNull]public static TagContent gEmpty(string tagName, params string[] properties)
        {
            return g(tagName, properties).Empty();
        }

        /// <summary>
        /// Create a blank tag. This is only used to intersperse plain text and tagged content in a parent
        /// </summary>
        [NotNull]public static TagContent g()
        {
            return new TagContent
            {
                Tag = null,
                IsEmpty = false,
                Properties = null
            };
        }
    }
}
