using System;
using System.Collections;
using System.Collections.Generic;
using Amazon.IonDotnet;

namespace Amazon.Ion.ObjectMapper
{
    public class IonListSerializer : IonSerializer<IList>
    {
        private readonly IonSerializer serializer;
        private Type listType;
        private Type elementType;
        private bool isGenericList;

        public IonListSerializer(IonSerializer serializer)
        {
            this.serializer = serializer;
        }

        public IonListSerializer(IonSerializer serializer, Type listType)
        {
            this.serializer = serializer;
            this.SetListType(listType);
        }
        
        internal void SetListType(Type listType)
        {
            if (listType == this.listType)
            {
                return;
            }

            this.listType = listType;

            if (listType.IsArray)
            {
                this.elementType = listType.GetElementType();
                this.isGenericList = true;
            }
            else if (listType.IsAssignableTo(typeof(IList)))
            {
                if (listType.IsGenericType)
                {
                    this.elementType = listType.GetGenericArguments()[0];
                    this.isGenericList = true;
                }
                else
                {
                    this.isGenericList = false;
                }
            }
            else
            {
                throw new NotSupportedException("Encountered an Ion list but the desired deserialized type was not an IList, it was: " + listType);
            }
        }

        public IList Deserialize(IIonReader reader)
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
                    typedArray.SetValue(list[i], i);
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
                    typedList.Add(element);
                }

                return typedList;
            }

            throw new NotSupportedException("Don't know how to make a list of type " + listType + " with element type " + elementType);
        }

        public void Serialize(IIonWriter writer, object item)
        {
            writer.StepIn(IonType.List);
            foreach (var i in (System.Collections.IList)item)
            {
                serializer.Serialize(writer, i);
            }
            writer.StepOut();
        }
    }
}