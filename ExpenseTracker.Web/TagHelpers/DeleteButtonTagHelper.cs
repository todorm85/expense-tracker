using Microsoft.AspNetCore.Razor.TagHelpers;

namespace ExpenseTracker.Web.TagHelpers
{
    [HtmlTargetElement(Attributes = "btn-del")]
    public class DeleteButtonTagHelper : TagHelper
    {
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.Attributes.Add("class", "btn btn-danger rounded-circle");
            output.Attributes.Add("style", "width:30px;height:30px");
            output.Content.SetHtmlContent("X");
            output.TagMode = TagMode.StartTagAndEndTag;
        }
    }
}