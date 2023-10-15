namespace Xigit
{
    public abstract class XElement
    {
        public int x, y;

        public XElement(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public abstract void Render(XWidget widget);
    }
}