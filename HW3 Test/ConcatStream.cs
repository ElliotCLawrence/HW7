/*Elliot Lawrence
 *ID 11349302
 *HW 5
 *CS422
 *10_6_2016
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CS422
{
    public class ConcatStream : Stream
    {
        private Stream firstStream, secondStream;
        private long maxLength;
        private long streamPosition;

        public ConcatStream(Stream first, Stream second)
        {
            firstStream = first;
            secondStream = second;
            maxLength = -1;
        }

        public ConcatStream(Stream first, Stream second, long fixedLength)
        {
            firstStream = first;
            secondStream = second;
            maxLength = fixedLength;
        }

        public override bool CanRead
        {
            get
            {
                if (!firstStream.CanRead || !secondStream.CanRead) //if either one cannot read, return false;
                {
                    return false;
                }
                return true;
            }
        }

        public override bool CanSeek
        {
            get
            {
                if (!firstStream.CanSeek || !secondStream.CanSeek) //if either one cannot seek, return false;
                {
                    return false;
                }
                return true;
            }
        }

        public override bool CanWrite
        {
            get
            {
                if (!firstStream.CanWrite || !secondStream.CanWrite) //if either one cannot write, return false;
                {
                    return false;
                }
                return true;
            }
        }

        public override long Length
        {
            get
            {
                if (maxLength > -1) //if maxLength was specified
                    return maxLength;
                else //else
                {
					return firstStream.Length + secondStream.Length;
                }
            }
        }

        public override long Position
        {
            get
            {
                return streamPosition;
            }

            set
            {
				if (this.CanSeek) { //only if you can seek on both streams....
					this.Seek (value, SeekOrigin.Begin);
					//streamPosition = value;
				} else {
					throw new Exception ();
				}
            }
        }

        public override void Flush()
        {
            firstStream.Flush();
            secondStream.Flush();

        }

        public override void SetLength(long value)
        {
            if (maxLength < 0)
                return;
            else
                maxLength = value;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (this.CanSeek) //only if you can seek on both streams....
            {
                if (origin == SeekOrigin.Begin) //from 0 (or beginning)
                    streamPosition = offset;
                
                else if (origin == SeekOrigin.Current) //from current position
                    streamPosition += offset;
               	
                else //seeking the end
                {
                    if (maxLength == -1)
                        throw new Exception(); //if max length not specified, throw error
                    streamPosition = maxLength + offset;
                }
                    
                if (streamPosition > firstStream.Length) //now do a sub seek within the two streams to set them up correctly
                {
                    firstStream.Seek(0, SeekOrigin.End);
                    secondStream.Seek(streamPosition - (firstStream.Length), SeekOrigin.Begin);
                }
                else
                {
                    firstStream.Seek(streamPosition, SeekOrigin.Begin);
                    secondStream.Seek(0, SeekOrigin.Begin);
                }

                return streamPosition;
            }

            else
                throw new NotSupportedException();
        }

       

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (! this.CanRead)//if this can't read, return 0
                return 0;

            //else do the read

            if (streamPosition > firstStream.Length) //if we need to read in the second stream
            {
                int newPosition = (int)(streamPosition - firstStream.Length); //calculate the appropriate position in the second stream
                int readVal = secondStream.Read(buffer, offset, count); //hold on to the ammount read in
                streamPosition += readVal; //increment the position by ammount read in
                return readVal; //return ammount read
            }

            //we're reading from both streams
            else if (streamPosition + count > firstStream.Length) //if this is true, we must read from stream one, and then some more from stream 2
            {
                int firstRead = (int) (firstStream.Length - streamPosition); //calculate how many bytes left in stream one to read.
                int bytesRead = firstStream.Read(buffer, offset, firstRead); //read to the end of stream one.
                count -= bytesRead;  //decrement count, offset, and streamposition
                offset += bytesRead;
                streamPosition += bytesRead;

                int secondBytesRead = secondStream.Read(buffer, offset, count); //read from stream two
                streamPosition += secondBytesRead; //increment streamPosition
                return bytesRead + secondBytesRead; //return total bytes read
            }

            else //only reading from the first stream
            {
                int readVal = firstStream.Read(buffer, offset, count);
                streamPosition += readVal;
                return readVal;
            }
        }

        public override void Write(byte[] buffer, int offset, int count) //is the offset the offset to buffer or to streamPosition
        {
            if (!firstStream.CanWrite || !secondStream.CanWrite) //if either of the streams cannot write, then return.
                throw new NotSupportedException();

            if (buffer.Length < count)
                throw new ArgumentException();


            if (streamPosition > firstStream.Length) //if only writing to the second stream
            {
                if (streamPosition - firstStream.Length != secondStream.Position)
                {
                    this.Seek(streamPosition, SeekOrigin.Begin);
                    //secondStream.Seek(streamPosition - firstStream.Length, SeekOrigin.Begin);
                }

                secondStream.Write(buffer, offset, count);
            }

            else if ( streamPosition + count > firstStream.Length) //writing to both streams
            {
                if (streamPosition != firstStream.Position) //set up the first stream for writing
                {
                    this.Seek(streamPosition, SeekOrigin.Begin);
                }
                if (secondStream.Position != 0) //set up second stream position for writing
                {
                    this.Seek(streamPosition, SeekOrigin.Begin);                   
                }

                //if you reach here, you're ready to write
                int writeDiff = (int)(firstStream.Length - streamPosition);
                firstStream.Write(buffer, offset, writeDiff);
                count -= writeDiff;
                streamPosition = firstStream.Length;

                secondStream.Write(buffer, offset + writeDiff, count);
                streamPosition += count;
                
            }
            
            else //reading from the first stream
            {
                if (streamPosition != firstStream.Position)
                {
                    this.Seek(streamPosition, SeekOrigin.Begin);
                }

                firstStream.Write(buffer, offset, count);
            }

        }
    }
}
