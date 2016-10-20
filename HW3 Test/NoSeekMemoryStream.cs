using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CS422
{
    class NoSeekMemoryStream : MemoryStream
    {
		public NoSeekMemoryStream(byte[] buffer) : base(buffer)
		{

		}

		public NoSeekMemoryStream(byte[] buffer, int offset, int count) : base(buffer, offset, count)
		{

		}
       
        public override long Position {
			get {
				return base.Position;
			}
			set {
				throw new Exception();
			}
		}

        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        public override long Seek(long offset, SeekOrigin loc)
        {
            throw new NotImplementedException();
        }
    }
}
