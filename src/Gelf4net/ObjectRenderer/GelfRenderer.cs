using log4net.ObjectRenderer;
using log4net.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gelf4net.ObjectRenderer
{
    public class GelfRenderer : IObjectRenderer
    {
        public void RenderObject(RendererMap rendererMap, object obj, System.IO.TextWriter writer)
        {
            var dictionary = obj as IDictionary;
            if (dictionary == null)
                writer.Write(SystemInfo.NullText);


        }
    }
}
