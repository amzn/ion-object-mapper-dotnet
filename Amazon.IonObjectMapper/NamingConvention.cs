namespace Amazon.IonObjectMapper
{
    public interface IonPropertyNamingConvention
    {
        public string ToProperty(string s);
        public string FromProperty(string s);
    }

    public class CamelCaseNamingConvention : IonPropertyNamingConvention
    {
        public string ToProperty(string s)
        {
            return s.Substring(0, 1).ToUpperInvariant() + s.Substring(1);
        }

        public string FromProperty(string s)
        {
            return s.Substring(0, 1).ToLowerInvariant() + s.Substring(1);
        }
    }

    public class TitleCaseNamingConvention : IonPropertyNamingConvention
    {
        public string ToProperty(string s)
        {
            return s.Substring(0, 1).ToUpperInvariant() + s.Substring(1);
        }

        public string FromProperty(string s)
        {
            return s.Substring(0, 1).ToUpperInvariant() + s.Substring(1);
        }
    }

    public class SnakeCaseNamingConvention : IonPropertyNamingConvention
    {
        public string FromProperty(string s)
        {
            var output = "";
            for (int i=0; i< s.Length; i++)
            {
                char c = s[i];
                if (char.IsUpper(c))
                {
                    if (i > 0)
                    {
                        output += "_";
                    }
                    output += char.ToLowerInvariant(c);
                }
                else
                {
                    output += c;
                }
            }
            return output;
        }

        public string ToProperty(string s)
        {
            if (s.Length == 0)
            {
                return "";
            }
            int i = 0;
            var output = "";
            if (s[0] == '_')
            {
                if (s.Length == 1) 
                {
                    i++;
                    output += "_";
                }
                else
                {
                    i += 2;
                    output += s.Substring(1, 1).ToUpperInvariant();
                }
            }
            else 
            {
                i++;
                output += s.Substring(0, 1).ToUpperInvariant();
            }
            for (; i< s.Length; i++)
            {
                char c = s[i];
                if (c == '_')
                {
                    if (i+1 < s.Length)
                    {
                        output += char.ToUpperInvariant(s[i+1]);
                        i++;
                    }
                }
                else
                {
                    output += c;
                }
            }
            return output;
        }
    }
}