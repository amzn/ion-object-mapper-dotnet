using System;
using System.Collections;
using System.Collections.Generic;
using Amazon.IonDotnet;

namespace Amazon.Ion.ObjectMapper
{
    public class IonListSerializer : IonSerializer<IList>
    {
        private readonly IonSerializer serializer;
        private readonly Type listType;
        private readonly Type elementType;
        private readonly bool isGenericList;

        public IonListSerializer(IonSerializer serializer, Type listType)
        {
            this.serializer = serializer;
            this.listType = listType;

            if (this.listType.IsArray)
            {
                this.elementType = this.listType.GetElementType();
                this.isGenericList = true;
            }
            else if (this.listType.IsAssignableTo(typeof(IList)))
            {
                this.isGenericList = this.listType.IsGenericType;
                if (this.isGenericList)
                {
                    this.elementType = this.listType.GetGenericArguments()[0];
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
            var list = new ArrayList();
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
            
            if (listType is IEnumerable || listType is object)
            {
                IList typedList;
                if (listType.IsGenericType) 
                {
                    typedList = (IList) Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));
                } else {
                    typedList = new ArrayList();
                }

                foreach (var element in list)
                {
                    typedList.Add(element);
                }

                return typedList;
            }

            throw new NotSupportedException("Don't know how to make a list of type " + listType + " with element type " + elementType);
        }

        public void Serialize(IIonWriter writer, IList item)
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
