using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Runtime.Serialization;

namespace InnoviApiProxy
{
    public class InnoviObjectCollection<InnoviElement> : ICloneable, IEnumerable<InnoviElement> where InnoviElement : InnoviObject
    {
        private InnoviObjectDelegate<InnoviElement> m_Delegate;
        private int m_FilterItemId;

        internal InnoviObjectCollection(InnoviObjectDelegate<InnoviElement> i_Delegate)
        {
            m_Delegate = i_Delegate;
        }

        object ICloneable.Clone()
        {
            return CloneImplementation();
        }

        public virtual InnoviObjectCollection<InnoviElement> Clone()
        {
            return CloneImplementation();
        }

        protected virtual InnoviObjectCollection<InnoviElement> CloneImplementation()
        {
            return new InnoviObjectCollection<InnoviElement>(m_Delegate, m_FilterItemId);
        }

        internal InnoviObjectCollection(InnoviObjectDelegate<InnoviElement> i_Delegate, int i_FilterItemId)
        {
            m_Delegate = i_Delegate;
            m_FilterItemId = i_FilterItemId;
        }

        public IEnumerator<InnoviElement> GetEnumerator()
        {
            return new InnoviObjectCollectionEnumerator(m_Delegate, m_FilterItemId);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new InnoviObjectCollectionEnumerator(m_Delegate, m_FilterItemId);
        }


        public List<InnoviElement> ToList()
        {
            List<InnoviElement> list = new List<InnoviElement>();

            foreach (InnoviElement innoviObject in this)
            {
                list.Add(innoviObject);
            }

            if (list.Count == 0)
            {
                list = null;
            }

            return list;
        }

        public bool IsEmpty()
        {
            bool res = true;

            foreach (InnoviElement innoviObject in this)
            {
                res = false;
                break;
            }

            return res;
        }

        internal class InnoviObjectCollectionEnumerator : IEnumerator<InnoviElement>
        {
            private int m_CurrentIndex = -1;
            private int m_CurrentPage = 0;
            private int m_TotalPages = 0;

            private List<InnoviElement> m_Collection;
            private InnoviObjectDelegate<InnoviElement> m_Delegate;

            private int m_FilterItemId;

            void IDisposable.Dispose()
            {

            }

            internal InnoviObjectCollectionEnumerator(InnoviObjectDelegate<InnoviElement> i_Delegate, int i_FilterItemId)
            {
                m_Delegate = i_Delegate;
                m_FilterItemId = i_FilterItemId;
            }

            public InnoviElement Current
            {
                get
                {
                    var res =  m_Collection[m_CurrentIndex];
                    return res;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    var res = m_Collection[m_CurrentIndex];
                    return res;
                }
            }

            public bool MoveNext()
            {
                object refObj = new object();
                bool refObjIsFirstTime = false;
                ObjectIDGenerator refObjIdGen = new ObjectIDGenerator();
                long refObjId = refObjIdGen.GetId(refObj, out refObjIsFirstTime);

                m_CurrentIndex++;

                if (m_Collection == null || m_CurrentIndex == m_Collection.Count)
                {
                    if (m_Collection == null)
                    {
                        m_Collection = new List<InnoviElement>();
                    }

                    m_CurrentPage++;

                    List<InnoviElement> currentSection = m_Delegate(m_FilterItemId, m_CurrentPage, out m_TotalPages);

                    if (currentSection != null)
                    {
                        m_Collection.AddRange(currentSection);
                    }
                }
                return m_CurrentPage <= m_TotalPages && m_CurrentPage < 10;
            }

            public void Reset()
            {
                m_CurrentIndex = -1;
                m_CurrentPage = 0;
                m_TotalPages = 0;
                m_Collection = null;
            }
        }
    }
}