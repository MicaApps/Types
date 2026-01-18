using System;
using System.Collections;
using System.Collections.Generic;

namespace Types.Core
{
    public class Verbs : IEnumerable<Verb>
    {
        public readonly Type Type;
        public readonly bool IsBackground;
        
        public Verbs(Type type, bool background)
        {
            Type = type;
            IsBackground = background;
        }
        
        private string RootPath
        {
            get
            {
                if (IsBackground) return Type.Link + "\\Background\\shell";
                else return Type.Link + "\\shell";
            }
        }
        
        public string GetValue(string path, string name)
        {
            string fullPath = RootPath;
            if (path != null) fullPath += "\\" + path;
            RegKey k = Type.CR.Open(fullPath);
            return k != null ? k[name] : null;
        }
        
        public void SetValue(string path, string name, string value)
        {
            string fullPath = RootPath;
            if (path != null) fullPath += "\\" + path;
            
            if (value != null) Type.WCU.Ensure(fullPath, true)[name] = value;
            else
            {
                if (Type.CU.HasKey(fullPath) && Type.CU.Open(fullPath).HasValue(name)) Type.WCU.Open(fullPath, true)[name] = null;
                if (Type.LM.HasKey(fullPath) && Type.LM.Open(fullPath).HasValue(name)) Type.WLM.Open(fullPath, true)[name] = null;
            }
        }
        
        public bool HasKey(string path)
        {
            string fullPath = RootPath + "\\" + path;
            return Type.CR.HasKey(fullPath);
        }
        
        public void NewKey(string path)
        {
            string fullPath = RootPath + "\\" + path;
            Type.WCU.Ensure(fullPath);
        }
        
        public void ZapKey(string path)
        {
            string fullPath = RootPath + "\\" + path;
            if (Type.CU.HasKey(fullPath)) Type.WCU.ZapKey(fullPath);
        }
        
        public string Default
        {
            get
            {
                RegKey k = Type.CR.Open(RootPath);
                return k != null ? k.Default : null;
            }
            set
            {
                Type.WCU.Ensure(RootPath, true).Default = value;
            }
        }
        
        public IEnumerator<Verb> GetEnumerator()
        {
            RegKey k = Type.CR.Open(RootPath);
            if (k != null)
            {
                foreach (string subKey in k.Keys)
                {
                    yield return new Verb(this, subKey);
                }
            }
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        public Verb this[string id]
        {
            get { return new Verb(this, id); }
        }
        
        public Verb AddVerbWithTitle(string title)
        {
            string id = title.Replace(" ", ""); // Simple sanitization
            if (string.IsNullOrEmpty(id)) id = "NewAction";
            NewKey(id);
            Verb v = new Verb(this, id);
            v.Title = title;
            return v;
        }
        
        public void Delete(string id)
        {
            ZapKey(id);
        }
    }
}
