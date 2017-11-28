using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Objects;

namespace Assets.Scripts.Collections
{
    public class SSCollection : List<SSObject>
    {
        public EventHandler CollectionModified;
        private readonly List<SSObject> _collection;

        public SSCollection()
        {
            _collection = new List<SSObject>();
        }

        public SSCollection(List<SSObject> collection)
        {
            _collection = collection;
        }

        public List<SSObject> GetCollection()
        {
            return _collection;
        }

        public SSObject GetObject(SSObject child)
        {
            lock (_collection)
            {
                return _collection.FirstOrDefault(c => c.Equals(child));
            }
        }

        public SSObject GetObject(int identifier)
        {
            lock (_collection)
            {
                return _collection.FirstOrDefault(c => c.Identifier.Equals(identifier));
            }
        }

        public SSObject GetFirstObject()
        {
            lock (_collection)
            {
                return _collection.FirstOrDefault();
            }
        }

        public new void Add(SSObject child)
        {
            lock (_collection)
            {
                _collection.Add(child);
            }
            CollectionModified?.Invoke(this, null);
        }

        public new void Remove(SSObject child)
        {
            lock (_collection)
            {
                _collection.Remove(child);
            }
            CollectionModified?.Invoke(this, null);
        }

        public new void Clear()
        {
            lock (_collection)
            {
                _collection.Clear();
            }
            CollectionModified?.Invoke(this, null);
        }

        public new int Count()
        {
            return _collection.Count;
        }
    }
}
