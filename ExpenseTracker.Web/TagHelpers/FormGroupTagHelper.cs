using Microsoft.AspNetCore.Razor.TagHelpers;

namespace ExpenseTracker.Web.TagHelpers
{
    [HtmlTargetElement("form-group", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class FormGroupTagHelper : TagHelper
    {
        public bool Inline { get; set; } = true;
        public string Class { get; set; } = "";

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            var classes = "form-group";
            
            if (Inline)
            {
                classes += " d-inline-block mr-2";
            }

            if (!string.IsNullOrEmpty(Class))
            {
                classes += " " + Class;
            }

            output.Attributes.SetAttribute("class", classes);

            // Add classes to child elements
            string labelClasses = "'mr-1'";
            string inputClasses = "'form-control', 'form-control-sm'";

            if (Inline)
            {
                labelClasses += ", 'd-inline-block'";
                inputClasses += ", 'w-auto', 'd-inline-block'";
            }

            output.PostElement.SetHtmlContent($@"<script>
                var el = document.currentScript.previousElementSibling;
                el.querySelector('label').classList.add({labelClasses});
                var input = el.querySelector('input, select');
                if(input) {{
                  input.classList.add({inputClasses});
                }}
                </script>");
        }
    }
}
