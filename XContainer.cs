using System.Collections.Generic;

namespace Xigit
{
    public class XContainer
    {
        List<XElement> elements = new List<XElement>();

        public XContainer()
        {
            elements = new List<XElement>();
        }

        public void AddElement(XElement element)
        {
            elements.Add(element);
        }

        public void Render(XWidget widget)
        {
            foreach (XElement element in elements)
            {
                element.Render(widget);
            }
        }
    }
}