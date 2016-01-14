using System.Xml.Serialization;

namespace Dashboard.Model.RSS
{
    /// <remarks/>
    [XmlType(AnonymousType = true)]
    public partial class RssChannel
    {
        private string m_Title;

        private string m_Link;

        private object m_Description;

        private RssChannelImage m_Image;

        private AtomLink m_Link1;

        private RssChannelItem[] m_Items;

        /// <remarks/>
        [XmlElement("title")]
        public string Title
        {
            get
            {
                return m_Title;
            }
            set
            {
                m_Title = value;
            }
        }

        /// <remarks/>
        [XmlElement("link")]
        public string Link
        {
            get
            {
                return m_Link;
            }
            set
            {
                m_Link = value;
            }
        }

        /// <remarks/>
        [XmlElement("description")]
        public object Description
        {
            get
            {
                return m_Description;
            }
            set
            {
                m_Description = value;
            }
        }

        /// <remarks/>
        [XmlElement("image")]
        public RssChannelImage Image
        {
            get
            {
                return m_Image;
            }
            set
            {
                m_Image = value;
            }
        }

        /// <remarks/>
        [XmlElement("link", Namespace = "http://www.w3.org/2005/Atom")]
        public AtomLink AtomLink
        {
            get
            {
                return m_Link1;
            }
            set
            {
                m_Link1 = value;
            }
        }

        /// <remarks/>
        [XmlElement("item")]
        public RssChannelItem[] Item
        {
            get
            {
                return m_Items;
            }
            set
            {
                m_Items = value;
            }
        }
    }
}