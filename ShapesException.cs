using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace ShapesGraphics
{
    [Serializable]
    public class ShapesException : ApplicationException
    {
        // My Data Additions:
        public string   MyMessage        { get; set; }
        public DateTime MyErrorTimeStamp { get; set; }

        // Given Constructors:
        public ShapesException() { }
        public ShapesException(string message) : base(message) { }
        public ShapesException(string message, Exception inner) : base(message, inner) { }
        protected ShapesException(SerializationInfo info, StreamingContext context) : base(info, context) { }      
        
         // My Constructor Addition:
        public ShapesException(string message, string MyMessage, DateTime date) : base(message)
        {            
            this.MyMessage = MyMessage;
            this.MyErrorTimeStamp = date;
        }
    }
}
