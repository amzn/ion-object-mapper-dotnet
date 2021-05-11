/*
 * Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License").
 * You may not use this file except in compliance with the License.
 * A copy of the License is located at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * or in the "license" file accompanying this file. This file is distributed
 * on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either
 * express or implied. See the License for the specific language governing
 * permissions and limitations under the License.
 */

namespace Amazon.Ion.ObjectMapper
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
            var output = string.Empty;
            for (int i = 0; i < s.Length; i++)
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
                return string.Empty;
            }

            int i = 0;
            var output = string.Empty;
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

            for (; i < s.Length; i++)
            {
                char c = s[i];
                if (c == '_')
                {
                    if (i + 1 < s.Length)
                    {
                        output += char.ToUpperInvariant(s[i + 1]);
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
