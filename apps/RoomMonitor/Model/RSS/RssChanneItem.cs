using System.Xml.Serialization;

namespace Dashboard.Model.RSS
{
    /// <remarks/>
    [XmlType(AnonymousType = true)]
    public partial class RssChannelItem
    {
        private string m_Title;

        private string m_Category;

        private string m_Link;

        private RssChannelItemGuid m_Guid;

        private string m_PubDate;

        private string m_Description;

        private string m_MediaTitle;

        private Thumbnail m_Thumbnail;

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
        [XmlElement("category")]
        public string Category
        {
            get
            {
                return m_Category;
            }
            set
            {
                m_Category = value;
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
        [XmlElement("guid")]
        public RssChannelItemGuid Guid
        {
            get
            {
                return m_Guid;
            }
            set
            {
                m_Guid = value;
            }
        }

        /// <remarks/>
        [XmlElement("pubDate")]
        public string PubDate
        {
            get
            {
                return m_PubDate;
            }
            set
            {
                m_PubDate = value;
            }
        }

        /// <remarks/>
        [XmlElement("description")]
        public string Description
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
        [XmlElement("title", Namespace = "http://search.yahoo.com/mrss/")]
        public string MediaTitle
        {
            get
            {
                return m_MediaTitle;
            }
            set
            {
                m_MediaTitle = value;
            }
        }

        /// <remarks/>
        [XmlElement("thumbnail", Namespace = "http://search.yahoo.com/mrss/")]
        public Thumbnail Thumbnail
        {
            get
            {
                return m_Thumbnail;
            }
            set
            {
                m_Thumbnail = value;
            }
        }
    }
}