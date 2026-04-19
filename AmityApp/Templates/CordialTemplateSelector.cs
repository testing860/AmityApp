using AmityApp.Models;
using AmityApp.Shared.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmityApp.Templates
{
    public class CordialTemplateSelector : DataTemplateSelector
    {
        public DataTemplate WithImage { get; set; }
        public DataTemplate WithNoImage { get; set; }
        public DataTemplate ImageOnly { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (item is CordialModel cordial)
            {
                if (string.IsNullOrWhiteSpace(cordial.PhotoUrl))
                {
                    // We have only text content
                    return WithNoImage;
                }

                if (string.IsNullOrWhiteSpace(cordial.Content))
                {
                    // We have only Photo/image
                    return ImageOnly;
                }
                // We have both
                return WithImage;
            }
            return null;
        }
    }
}
