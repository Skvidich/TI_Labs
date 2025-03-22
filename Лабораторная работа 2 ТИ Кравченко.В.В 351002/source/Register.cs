using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace lab2TI
{
    class Register
    {
        int Size;
        List<byte> FeedbackPos;
        List<byte> State;


        public Register(int size,List<byte> pos,List<byte> state)
        {
            Size = size;
            FeedbackPos = pos;
            State = state;
        }

        public byte DoShift()
        {
            byte res = State[0];
            Console.WriteLine(State.ToString()+"\n");
            byte temp = 0;
            for(var i = 0; i < FeedbackPos.Count; i++)
            {
                temp = (byte)(temp ^ State[FeedbackPos[i]]);
            }

            for (int i = 0; i < Size-1; i++)
            {
                State[i] = State[i + 1];
            }
            State[Size - 1] = temp;
            return res;

        }
    }
}
