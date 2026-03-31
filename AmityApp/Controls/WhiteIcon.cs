using CommunityToolkit.Maui.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmityApp.Controls
{
        public class WhiteIcon : Image
    {

        public WhiteIcon()
        {
            var tintColorBehavior = Behaviors.FirstOrDefault(b => b is IconTintColorBehavior);
            if (tintColorBehavior == null)

            {
                tintColorBehavior = new IconTintColorBehavior
                {
                    TintColor = Colors.White
                };
                Behaviors.Add(tintColorBehavior);

            }

        }

    }
}
