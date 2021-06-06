using System;
using System.Collections.Generic;
using Amazon.IonDotnet;

namespace Amazon.Ion.ObjectMapper
{
    public class IonListSerializer : IonSerializer<System.Collections.IList>
    {
        private readonly IonSerializer serializer;
        private readonly Type listType;
        private readonly Type elementType;
        private readonly bool isGenericList;
        private readonly bool permissiveMode;

        public IonListSerializer(IonSerializer serializer, Type listType, Type elementType, bool permissiveMode)
        {
            this.serializer = serializer;
            this.listType = listType;
            this.elementType = elementType;
            this.isGenericList = true;
            this.permissiveMode = permissiveMode;
        }

        public IonListSerializer(IonSerializer serializer, Type listType, bool permissiveMode)
        {
            this.serializer = serializer;
            this.listType = listType;
            this.isGenericList = false;
            this.permissiveMode = permissiveMode;
        }

        public override System.Collections.IList Deserialize(IIonReader reader)
        {
            reader.StepIn();
            var list = new System.Collections.ArrayList();
            IonType ionType;
            while ((ionType = reader.MoveNext()) != IonType.None)
            {
                list.Add(serializer.Deserialize(reader, elementType, ionType));
            }
            reader.StepOut();
            
            if (listType.IsArray)
            {
                var typedArray = Array.CreateInstance(elementType, list.Count);
                for (int i=0; i<list.Count; i++)
                {
                    try
                    {
                        typedArray.SetValue(list[i], i);
                    }
                    catch (Exception)
                    {
                        if (permissiveMode)
                        {
                            typedArray.SetValue(Activator.CreateInstance(elementType), i);
                        }
                        else
                        {
                            throw;
                        }
                    }

                }
                return typedArray;
            }
            
            if (listType is System.Collections.IEnumerable || listType is object)
            {
                System.Collections.IList typedList;
                if (listType.IsGenericType) 
                {
                    typedList = (System.Collections.IList) Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));
                } else {
                    typedList = new System.Collections.ArrayList();
                }

                foreach (var element in list)
                {
                    try
                    {
                        typedList.Add(element);
                    }
                    catch (Exception)
                    {
                        if (permissiveMode)
                        {
                            typedList.Add(Activator.CreateInstance(elementType));
                        }
                        else
                        {
                            throw;
                        }
                    }
                }

                return typedList;
            }

            throw new NotSupportedException("Don't know how to make a list of type " + listType + " with element type " + elementType);
        }

        public override void Serialize(IIonWriter writer, System.Collections.IList item)
        {
            writer.StepIn(IonType.List);
            foreach (var i in item)
            {
                serializer.Serialize(writer, i);
            }
            writer.StepOut();
        }
    }
}