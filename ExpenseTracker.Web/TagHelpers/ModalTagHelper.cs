using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Threading.Tasks;

namespace ExpenseTracker.Web.TagHelpers
{
    public class ModalTagHelper : TagHelper
    {
        public string Title { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var childContent = await output.GetChildContentAsync();
            output.TagName = "div";
            output.Attributes.Add("class", "modal fade");
            output.Attributes.Add("tabindex", "-1");
            output.Attributes.Add("aria-labelledby", "exampleModalLabel");
            output.Attributes.Add("aria-hidden", "true");
            output.Content.SetHtmlContent(
   $@"<div class=""modal-dialog"">
            <div class=""modal-content"">
                <div class=""modal-header"">
                    <h5 class=""modal-title"" id=""exampleModalLabel"">{Title}</h5>
                    <button type = ""button"" class=""close"" data-dismiss=""modal"" aria-label=""Close"">
                        <span aria-hidden=""true"">&times;</span>
                    </button>
                </div>
                <div class=""modal-body"">
                    {childContent.GetContent()}
                </div>
            </div>
        </div>");
            output.TagMode = TagMode.StartTagAndEndTag;
        }
    }
}