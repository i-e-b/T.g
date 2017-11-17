using System;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Tag.Tests
{
    [TestFixture]
    public class TagTests
    {
        [Test]
        public void simple_tag_output()
        {
            var expected =    "<div class=\"glass\">Hello, world</div>";
            var actual = T.g("div", "class","glass")["Hello, world"].ToString();

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void empty_tag()
            {
            var expected = "<br/>";
            var actual = T.g("br").Empty().ToString();

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void empty_tag_other_way()
        {
            var expected = "<br/>";
            var actual = T.gEmpty("br").ToString();

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void empty_tag_a_third_way()
        {
            var expected = "<br/>";
            var actual = T.g("br/").ToString();

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void tag_with_tag_as_contents(){
            var expected =  "<div class=\"glass\"><a href=\"#\">Fish</a></div>";
            var subject = T.g("div", "class","glass")[
                            T.g("a", "href", "#")["Fish"]
                          ];

            var actual = subject.ToString();
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void tag_containing_multiple_children(){
            var expected = "<div><i>1</i><br/><i>2</i></div>";

            var subject = T.g("div")[ T.g("i")["1"], T.gEmpty("br"), T.g("i")["2"] ];
            
            var actual = subject.ToString();
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void tag_containing_multiple_children_and_plain_content(){
            var expected = "<div>1<p>2</p>3</div>";

            var subject = T.g("div")[ T.g()["1"], T.g("p")["2"], T.g()["3"] ];
            
            var actual = subject.ToString();
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void tag_containing_multiple_children_and_plain_content_with_implicit_conversion(){
            var expected = "<div>1<p>2</p>3</div>";

            var subject = T.g("div")[ "1", T.g("p")["2"], "3" ];
            
            var actual = subject.ToString();
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void write_to_a_text_writer() {
            var expected = "<div>1<p>2</p>3</div>";

            var outp = new StringWriter();
            var subject = T.g("div")[ "1", T.g("p")["2"], "3" ];

            subject.StreamTo(outp);

            Assert.That(outp.ToString(), Is.EqualTo(expected));
        }

        [Test]
        public void write_to_a_byte_stream(){
            var expected = "<div>1<p>2</p>3</div>";

            var outp = new MemoryStream();
            var subject = T.g("div")[ "1", T.g("p")["2"], "3" ];

            subject.StreamTo(outp, Encoding.ASCII);
            var rawData = outp.ToArray();

            var result = Encoding.ASCII.GetString(rawData);
            Assert.That(result, Is.EqualTo(expected));
        }
        
        [Test]
        public void write_to_a_text_writer_multiple_times() {
            var expected = "<div>yo</div><div>yo</div><div>yo</div>";

            var outp = new StringWriter();
            var subject = T.g("div")[ "yo" ];

            subject.StreamTo(outp);
            subject.StreamTo(outp);
            subject.StreamTo(outp);

            Assert.That(outp.ToString(), Is.EqualTo(expected));
        }
        
        [Test]
        public void write_to_a_byte_stream_multiple_times(){
            var expected = "<div>yo</div><div>yo</div><div>yo</div>";

            var outp = new MemoryStream();
            var subject = T.g("div")[ "yo" ];

            subject.StreamTo(outp, Encoding.ASCII);
            subject.StreamTo(outp, Encoding.ASCII);
            subject.StreamTo(outp, Encoding.ASCII);

            var rawData = outp.ToArray();

            var result = Encoding.ASCII.GetString(rawData);
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void tag_contents_are_joined_with_text_contents()
        {
            var subject = T.g("p")["Hello"];

            var one = subject.ToString();

            subject.Add("Bingo", T.g("i")["Bango"]);
            var two = subject.ToString();

            Assert.That(one, Is.EqualTo("<p>Hello</p>"));
            Assert.That(two, Is.EqualTo("<p>HelloBingo<i>Bango</i></p>"));
        }

        [Test]
        public void can_update_child_contents_after_wrapping_in_parent()
        {
            var inner = T.g("p")["Hello"];
            var parent = T.g("div")[T.g("span")[inner]];

            inner.Add(", ");
            inner.Add("World");

            var expected = "<div><span><p>Hello, World</p></span></div>";
            Assert.That(parent.ToString(), Is.EqualTo(expected));
        }

        [Test]
        public void can_include_a_single_tag_multiple_times() {
            var br = T.g("br/");
            var subject = T.g("div")["These",br,"are",br,"on",br,"multiple",br,"lines"];

            var expected = "<div>These<br/>are<br/>on<br/>multiple<br/>lines</div>";

            Assert.That(subject.ToString(), Is.EqualTo(expected));
        }

        [Test]
        public void can_include_a_deep_content_multiple_times(){
            var content = T.g()[
                T.g("span","class","bold")["Now"],
                T.g("p")["is the winter of our discontent"],
                T.g("p")["Made glorious summer by this sun of York;"],
                T.g("p")[T.g("i")["--Gloucester"]]
                ];

            var subject = T.g()[
                    T.g("div","class","style1")[content],
                    T.g("div","class","style2")[content]
                ];

            var expected = "<div class=\"style1\"><span class=\"bold\">Now</span><p>is the winter of our discontent</p><p>Made glorious summer by this sun of York;</p><p><i>--Gloucester</i></p></div>" +
                           "<div class=\"style2\"><span class=\"bold\">Now</span><p>is the winter of our discontent</p><p>Made glorious summer by this sun of York;</p><p><i>--Gloucester</i></p></div>";

            var actual = subject.ToString();
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void using_preset_attributes(){
            var attrs = new []{"class", "myStyle", "selectable", "false" };
            var subject = T.g("outer", attrs)[T.g("inner/", attrs)];

            var expected = "<outer class=\"myStyle\" selectable=\"false\"><inner class=\"myStyle\" selectable=\"false\"/></outer>";

            Assert.That(subject.ToString(), Is.EqualTo(expected));
        }

        [Test]
        public void using_a_function_for_attributes() {
            Func<int, string[]> attrs = (id) => new []{ "class","direct","id","dt_"+id,"name","direct #"+id};

            var subject = T.g()[
                T.g("div", attrs(1)),
                T.g("div", attrs(2))
                ];

            var expected = "<div class=\"direct\" id=\"dt_1\" name=\"direct #1\"></div><div class=\"direct\" id=\"dt_2\" name=\"direct #2\"></div>";

            Assert.That(subject.ToString(), Is.EqualTo(expected));
        }

        [Test]
        public void adding_tag_content_from_linq()
        {
            var src = new[] { "a", "b", "c" };

            var subject = T.g("x");
            subject.Add(src.Select(s => T.g()[s]));

            Assert.That(subject.ToString(), Is.EqualTo("<x>abc</x>"));
        }

        [Test]
        public void setting_tag_content_from_linq()
        {
            var src = new[] { "a", "b", "c" };

            var subject = T.g("x")[src.Select(s => T.g()[s]).ToArray()]; // note it must be an array, not a list or enumerable
            //var subject = T.g("x")[src];  // this won't work, C# can't see the cast from string->TagContent behind the array

            Assert.That(subject.ToString(), Is.EqualTo("<x>abc</x>"));
        }

        [Test]
        public void can_load_a_file_as_tag_content() {
            var expected = "<p>This is a sample content file <i>with</i> tags.</p>"; //make sure this is the same as the sample file

            var cwd = BaseLocation();
            var subject = T.g("p").LoadFile(Path.Combine(cwd, "SampleFile.txt"));

            Assert.That(subject.ToString(), Is.EqualTo(expected));
        }

        /// <summary>
        /// Return the compiled location of the DLL, as test harnesses often move things around
        /// </summary>
        private string BaseLocation(){
            return Path.GetDirectoryName(GetType().Assembly.CodeBase.Replace("file:///","")) ?? "";
        }
    }
}