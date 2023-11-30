using System.Reflection;
using System.Text;

namespace Stribute
{
    internal class Program
    {

        static void Main(string[] args)
        {
            var s = ObjectToString(new TestClass(15, "STR", 2.2m, new char[] { 'A', 'B', 'C' }));
            Console.WriteLine(s);

        }
        [AttributeUsage(AttributeTargets.Property)]
        class CustomNameAttribute : Attribute
        {
            public string CustomFieldName { get; set; }

            public CustomNameAttribute(string customFieldName)
            {
                CustomFieldName = customFieldName;
            }
        }

        static string ObjectToString(object o)
        {
            StringBuilder sb = new StringBuilder();
            Type t = o.GetType();
            sb.Append(t.AssemblyQualifiedName + ":");
            sb.Append(t.Name + "|");
            var properties = t.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var p in properties)
            {
                if (p.GetCustomAttribute(typeof(CustomNameAttribute)) != null)
                    continue;

                var value = p.GetValue(o);
                var attributeName = "";

                var customNameAttribute = p.GetCustomAttribute(typeof(CustomNameAttribute)) as CustomNameAttribute;
                if (customNameAttribute != null)
                    attributeName = customNameAttribute.CustomFieldName;
                else
                    attributeName = p.Name;

                sb.Append(attributeName + ":");
                if (p.PropertyType == typeof(char[]))
                {
                    sb.Append(new string(value as char[]) + "|");
                }
                else
                    sb.Append(value + "|");
            }

            return sb.ToString();
        }
        static object StringToObject(string s)
        {
            var values = s.Split("|");
            var classAssemblyAndName = values[0].Split(':');

            var obj = Activator.CreateInstance(null, classAssemblyAndName[1])?.Unwrap();

            if (values.Length > 1 && obj != null)
            {
                var type = obj.GetType();

                for (int i = 1; i < values.Length; i++)
                {
                    var nameAndValue = values[i].Split(":");
                    var propertyName = nameAndValue[0];
                    var propertyValue = nameAndValue[1];
                    var pi = type.GetProperty(propertyName);

                    if (pi == null)
                    {
                        foreach (var p in type.GetProperties())
                        {
                            var customNameAttribute = p.GetCustomAttribute(typeof(CustomNameAttribute)) as CustomNameAttribute;
                            if (customNameAttribute != null && customNameAttribute.CustomFieldName == propertyName)
                            {
                                pi = p;
                                break;
                            }
                        }
                    }

                    if (pi == null)
                        continue;

                    if (pi.PropertyType == typeof(int))
                    {
                        pi.SetValue(obj, int.Parse(propertyValue));
                    }
                    if (pi.PropertyType == typeof(string))
                    {
                        pi.SetValue(obj, propertyValue);
                    }
                    if (pi.PropertyType == typeof(decimal))
                    {
                        pi.SetValue(obj, decimal.Parse(propertyValue));
                    }
                    if (pi.PropertyType == typeof(char[]))
                    {
                        pi.SetValue(obj, propertyValue.ToCharArray());
                    }
                }
            }

            return obj;
        }

        class TestClass
        {
            [CustomName("CustomFieldName")]
            public int I { get; set; }
            private string? S { get; set; }
            public decimal D { get; set; }
            public char[]? C { get; set; }

            public TestClass()
            { }
            public TestClass(int i)
            {
                this.I = i;
            }
            public TestClass(int i, string s, decimal d, char[] c) : this(i)
            {
                this.S = s;
                this.D = d;
                this.C = c;
            }
        }
    }
}

