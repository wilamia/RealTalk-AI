using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealTalk_AI.Services
{
    public class ChatItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate StringTemplate { get; set; }
        public DataTemplate SearchResultTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            return item is ChatSearchResult ? SearchResultTemplate : StringTemplate;
        }
    }
}
