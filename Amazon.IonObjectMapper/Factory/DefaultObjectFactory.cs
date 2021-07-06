/*
 * Copyright (c) Amazon.com, Inc. or its affiliates. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"). You may not use this file except in compliance with
 * the License. A copy of the License is located at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * or in the "license" file accompanying this file. This file is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR
 * CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
 * and limitations under the License.
 */

namespace Amazon.IonObjectMapper
{
    using System;
    using System.Reflection;
    using Amazon.IonDotnet;

    public class DefaultObjectFactory : IObjectFactory
    {
        public object Create(IonSerializationOptions options, IIonReader reader, Type targetType)
        {
            var annotations = reader.GetTypeAnnotations();
            if (annotations.Length > 0)
            {
                var typeName = annotations[0];
                Type typeToCreate = null;

                if (options.AnnotatedTypeAssemblies != null)
                {
                    foreach (string assemblyName in options.AnnotatedTypeAssemblies)
                    {
                        if ((typeToCreate = Type.GetType(FullName(typeName, assemblyName))) != null)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        typeToCreate = assembly.GetType(assembly.GetName().Name + "." + typeName);
                        if (typeToCreate != null)
                        {
                            break;
                        }
                    }
                }

                if (typeToCreate != null && targetType.IsAssignableFrom(typeToCreate))
                {
                    return Activator.CreateInstance(typeToCreate);
                }
            }

            return Activator.CreateInstance(targetType);
        }

        private static string FullName(string typeName, string assemblyName)
        {
            return assemblyName + "." + typeName + "," + assemblyName;
        }
    }
}
