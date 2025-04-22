using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Claims;
namespace TI3
{
    

    class ElGamalCrypto
    {
        public int P; 
        public List<int> GList;
        public int G;
        public int X; 
        public int Y; 

        public ElGamalCrypto(int p, int x)
        {
            P = p;
            X = x;
            GList = FindPrimitiveRoots(p); 
        }

        public bool SetG(int g)
        {
            if (!GList.Contains(g))
            {
                return false;
            }
            G = g;
            return true;
        }

        public void SetY()
        {
            Y = ModPow(G, X, P);
        }

        public (int,int) EncryptByte(byte m, int k)
        {
            int a = ModPow(G, k, P);
            int b = (ModPow(Y, k, P) * m) % P;
            return (a, b);
        }

        public byte DecryptByte(int a, int b)
        {
            int s = ModPow(a, X, P);
            int sInv = ModInverse(s, P);
            int m = (b * sInv) % P;
            return (byte)m;
        }

        public MemoryStream EncryptFile(MemoryStream input,  int k)
        {
            var output = new MemoryStream();
            
            for (int OutByte = input.ReadByte(); OutByte != -1; OutByte = input.ReadByte())
            {
                if (OutByte >= P)
                {
                    throw new Exception("значение байта больше либо равно p");
                }
                var (a, b) = EncryptByte((byte)OutByte, k);
                byte[] intBytes = BitConverter.GetBytes(a);
                output.Write(intBytes, 0, intBytes.Length);

                intBytes = BitConverter.GetBytes(b);
                output.Write(intBytes, 0, intBytes.Length);
            }
            output.Position = 0;
            return output;
        }

        public MemoryStream DecryptFile(MemoryStream input)
        {
            var output = new MemoryStream();
            byte[] buffer = new byte[4];
            for (int i = input.Read(buffer, 0, 4); i != 0; i = input.Read(buffer, 0, 4))
            {
                int a = BitConverter.ToInt32(buffer, 0);
                input.Read(buffer, 0, 4);
                int b = BitConverter.ToInt32(buffer, 0);
                byte m = DecryptByte(a, b);
                output.WriteByte(m);
            }
            output.Position = 0;
            return output;

        }

        public static int ModPow(int a, int b, int m)
        {
            int result = 1;

            while (b > 0)
            {
                while (b % 2 ==0) 
                {
                    b /= 2;
                    a= (a*a) % m;
                }

                result = (result*a) % m;
                b--;
            }

            return result;
        }


        public static int ModInverse(int a,int mod)
        {
            int m0 = mod, t, q;
            int x0 = 0, x1 = 1;

            if (mod == 1) return 0;

            while (a > 1)
            {
                q = a / mod;
                t = mod;

                mod = a % mod; a = t;
                t = x0;

                x0 = x1 - q * x0;
                x1 = t;
            }

            return x1 < 0 ? x1 + m0 : x1;
        }

        public static List<int> FindPrimitiveRoots(int p)
        {
            List<int> roots = new();
            int phi = p - 1;
            var factors = PrimeFactors(phi);

            for (int g = 2; g < p; g++)
            {
                bool flag = true;
                foreach (var q in factors)
                {
                    if (ModPow(g, phi / q, p) == 1)
                    {
                        flag = false;
                        break;
                    }
                }
                if (flag) roots.Add(g);
            }
            return roots;
        }

        public static HashSet<int> PrimeFactors(int n)
        {
            HashSet<int> factors = new();
            for (int i = 2; i * i <= n; i++)
            {
                while (n % i == 0)
                {
                    factors.Add(i);
                    n /= i;
                }
            }
            if (n > 1) factors.Add(n);
            return factors;
        }
        public static bool isPrime(int val)
        {
            if (val <= 1) return false;
            if (val == 2) return true;
            if (val % 2 == 0) return false;

            int boundary = (int)Math.Floor(Math.Sqrt(val));

            for (int i = 3; i <= boundary; i += 2)
            {
                if (val % i == 0)
                    return false;
            }

            return true;
        }

        public static bool isRelativelyPrime(int n, int m)
        {
            return EuclieandEx(n, m) == 1;
        }
        private static int EuclieandEx(int a, int b)
        {
            int d0 = a, d1 = b;
            int x0 = 1, x1 = 0;
            int y0 = 0, y1 = 1;
            int q, d2, x2, y2;
            while (d1 != 0)
            {
                q = d0 / d1;
                d2 = d0 % d1;
                d0 = d1;
                d1 = d2;

                x2 = x0 - q * x1;
                x0 = x1;
                x1 = x2;

                y2 = y0 - q * y1;
                y0 = y1;
                y1 = y2;
            }
            return d0;
        }

    }

}
