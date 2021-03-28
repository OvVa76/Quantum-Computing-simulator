/* Author: Ovidiu Vancu, mailto: ovidiu01.vancu@gmail.com
 * Warranty info: 
 * This SW code comes with no warranty whatsoever.It was created for learning purpose only, and not for commercial use.
 * It can be modified, extended, used in other applications only if any meaningful outcome is published here, so that anyone else can benefit.
 * Please do not republish this meterial on other sites without author's consent.
 * 
 * Purpose:
 * This code creates a simulator for a quantum registry and allows the user to manipulate it in different ways.
 * Unlike other simulators, this is a C# library, so it can be mixed up with clasical code very easy.
 * if you are totally new to quantum computing, then first please read some materials available on internet, in order to get aquainted to the basic notions
 * if there is anything wrong in the below code / comments, please feel free to correct me.
 * This concept is from scratch and not based or inspired from any other sources, so some of the theoretical concepts could be wrongly implemented,
 * and might need to be corrected.
 * 
 * How to use:
 * Create a C# console app. Dump this code there and then check the main() function at the bottom of the code, 
 * and decomment the tests that you want to run.
 * */

using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Security.Cryptography;

namespace VirtualQuantumComputing
{
    public class Pair
// this class implements the ket (a, b). I know, the representation is vertical, but is very hard to do this in comments here.
// so all ket representations in this material will be only horisontal. But I will not use any bra notation, so there is no risk of confusion
    {
        private static double errorM = 0.00001;
        private static double unupedoi = 0.5;
        private static double treipepatru = 0.75;
        private bool valid;
        private double a;
        private double b;

        private double laPatrat(double p)
        // This function will calculate the pow 2 and then round it.
        // the ket values a and b tends to have irregular values, like 1/sqrt(2). but the probability to generate a 1 or 0 is a pow 2 and b pow 2
        // so when the vqbit is in equal superposition state, I want the probability to generate a 1 to be 0.5 and not 0.499999.
        {
            double var = p * p;
            if (Math.Abs(unupedoi - var) < errorM)
                return unupedoi;
            if (Math.Abs(treipepatru - var) < errorM)
                return treipepatru;
            if (Math.Abs(1 - treipepatru - var) < errorM)
                return 1 - treipepatru;
            if (Math.Abs(1 - var) < errorM)
                return 1;
            if (Math.Abs(var) < errorM)
                return 0;
            Console.WriteLine("LaPatrat: Valoare invalida: p=" + p + " var =" + var);
            return -1;
        }

        //constructor
        public Pair(double inputA, double inputB)
        {
            if (laPatrat(inputA)+laPatrat(inputB) != 1)
            {
                valid = false;
                Console.WriteLine("Pair constructor: Invalid value a=" + inputA + ", b=" + inputB);
            }
            else changeVal(inputA, inputB);
        }

        // changes the orginal values from the pair with new ones. This function is called evry time a quantum gate is applied on a vqbit
        public bool changeVal(double anou, double bnou)
        {
            valid = true;
            if (laPatrat(anou) + laPatrat(bnou) != 1)
            {
                Console.WriteLine("ChangeVal: Invalid values. anou=" + anou + ", bnou=" + bnou + " a2=" + laPatrat(anou) + " b2=" + laPatrat(bnou));
                valid = false;
            }
            if (valid)
            {
                a = anou;
                b = bnou;
            }
            return valid;
        }

        public double getA()
        {
            return a;
        }

        public double getB()
        {
            return b;
        }

// when I want to know the state, in general I want to know the probability to generate a 0 or 1 and not its square root.
// the function return the probability to generate a 0. the probability to generate a 1 is 1-state
        public double getState()
        {
            return laPatrat(a);
        }

        public bool isValid()
        {
            return valid;
        }
    } // end Pair



    // this class implements the concept of quantum phase. The phase does not directly influence the probability to generate a 0 or 1
    // so I resisted the temptation to link it to (a, b) ket like in the mathematical description (see the (T) - PI/8 gate) and instead chosed 
    // to manipulate it as a distinct entity
    public class Phase
    {
        const int MAXSize = 100; // max size of the quantum registry. It can me more, but it would be hard to track the calculations
/* the phase formula is something like: e (Euler) to pow((2*i(sqr(-1)*PI)/2 pow m) - please check the documentation on internet for a more clear description
 * multiple phase shifts can be cummulated by a qbit (eg. the Quantum Fourier Transform) that will result in something like:
 * (e pow(2*PI*i/(2 pow m1))*(e pow(2*PI*i/(2 pow m2)* ....   that can be more conveniently represented as:
 * e pow ((2*PI*i/(2 pow maxM))*(2 pow (maxM - m1)+ 2 pow (maxM - m2) .... ). It's easy to see if you try it on paper
 * since e pow 2*PI*i is the constant part of the expression, it is not represented in this class, but assumed implicitly. 
 * in addition, e pow 2*PI*i  = 1
 */
        private uint m; // this is the maxM, I will call it the base
        private int[] num; //this is the vector of maxM - m1,2 ....
        private uint numSize; // always equal to the registry size

        public Phase(uint size)
        {
            if ((size < 1) || (size > MAXSize))
            {
                Console.WriteLine("Phase constructor: invalid register size: " + size);
                return;
            }
            m = 0; // when the phase is initialized, m = 0, e pow 2*PI*i/2 pow 0 = 1, so no phase basically
            numSize = size; 
            num = new int[size];
            for (int i = 0; i < size; i++)
                num[i] = -1;
        }

        private void reduction()
// let's suppose the phase is (1+4)/8 and there is a shift of 1/8. Then resulting phase is (1+1+4)/8 = (1+2)/4
// So the base is reduced from 2 pow 3 to 2 pow 2
// this function does exactly this
        {
            uint idx = 0;
            for (int i = 0; i < m; i++)
            {
                if (num[i] == 1) //num[i] = 1 indicates a 2pow m + 2 pow m situation. This equals to 2 pow m+1
                {
                    num[i] = -1;
                    num[i + 1]++;
                }
            }
            if (num[m] >= 0) num[m] = -1; // e pow 2*PI*i = 1, so it can be elliminated
            // next the base can be reduced to m-idx  and the num vector shifted to the left with m - idx
            while ((num[idx] == -1) && (idx < m)) idx++;
            if ((idx > 0) && (num[idx] >= 0))
            {
                for (uint i = idx; i < m; i++)
                {
                    num[i - idx] = num[i];
                    num[i] = -1;
                }
                m = m - idx;
            }
        }

// shifts the phase with e pow 2*PI*i / 2 pow val
        public void shiftPhase(uint val)
        {
            if ((val < 0) || (val > MAXSize))
            {
                Console.WriteLine("shiftPhase: invalid imput param: " + val);
                return;
            }
            if (val == 0) return; 

            if (m == 0) // this is the first phase shift, the phase is now e pow 2*PI*i / 2 pow val
            {
                m = val;
                num[0] = 0;
                return;
            }
// let's say Phase is now (1+4) / 8 and the shift is 1/16. The new base becomes 16
// and new phase is (1+2+8)/16
// if the shift is 1/4, the base is not changed, the new phase is (1+2+4)/8
// if the shift is 1/8, then the new phase is (2+4)/8 = (1+2)/4 following the reduction
            if (val > m) 
            {
                for (int i = (int)m - 1; i >= 0; i--)
                {
                    num[i+val-m] = num[i];
                    num[i] = -1;
                }
                m = val;
                num[0] = 0;
            }
            else
            {
                num[m-val]++;
                reduction();
            }
        }

        public int getSign()
        {
            if (m % 2 == 0) return 1;
            return -1;
        }

        public int[] returnNum()
        {
            return num;
        }

        public uint returM()
        {
            return m;
        }

        public static uint getMaxSize()
        {
            return MAXSize;
        }
    }

/* Virtual qbit basis class
 * it implements the simultation for the qbit concept, including several quantum gates
 * 
 * besides the clasical equal superposition state, this class introduces also a unequal superposition (sqrt(3)/2, 1/2) and (1/2, sqrt(3)/2)
 * corresponding to a probability to generate 0 and 1 of (0.75, 0.25) respective (0.25, 0.75)
 * this indicates something like: this value is more likely to be 0 then 1.
 * this state is not covered in the mathematical description of a qbit, but I created it during my totally unsuccesfull attempt to create a Oracle for
 * inverting the md5 password encryption function. I chosed to keep it as I still see the concept usefull, even if now it doesn't have a correspondent
 * with the real quantum bits. 
 * 
 * The idea of inverting an uninvertible function comes from the Grover's algorithm. The core of the algorithm is this Oracle
 * which, in the case of inverting the md5, for eg, is capable of running the md5 hash function on a vector of qbits in superposition 
 * (instead of a normal vector of bits in the real world) and  * then compare the resulting superposition to the encripted password we try to decript. 
 * All of these happening in just one quantum operation. And the result would be that the oracle is marking the particular vector x in the superposition
 * that we are looking for, by negating its amplitude.
 * Eg: 2 qbit can hold the numbers 0, .. 3, when in superposition. if we want to mark the value 2  ( which is (0, 1) (1, 0)), we could apply PZG on 
 * first qbit (but this will negate the amplitude for both 2 and 3) and apply a phase shift to negate the negation of 3 (knowing that e pow PI*i = -1)
 * For some reason that I can't understand, after implementing a quantum md5 hash, the Oracle would not able to apply Hadamard on resulting configuration 
 * that will move the registry to the searched configuration. Instead, Grover's alghorithm proposes a set of quantum calculations that will slowly 
 * amplify the amplitude of the targeted configuration while decreasing all others to 0.
 * If anyone can explain why the Oracle would fail short of applying one more gate to reveal the result and instead the Grover's diffusion operator
 * would be needed, please be so kind and leave a comment
 * 
 * IMHO, currently the quantum programming is somewhere at ant level, when compared to humans. It is still missing basic concepts like adding to registry.
 * So this VQBit basis class will only implement the basic operations on qubit, hoping that some meaningfull operations, like addition or substractions
 * can be constrcted based in it.
 */
    public class VQbitBasis
    {
        protected uint regSize; // registry size
        protected static int initInt = 0;
        protected Pair crtState;
        protected bool valid;
        protected bool activ;
        protected VQbitBasis entangled; // the address of the vqubit that this is entangled to or null
        protected bool entType; // true if entanglement type is direct (the vqbits have the same value) or opposit (the vqubits have opposite values)
        protected Phase ph;

        protected virtual void setActive()
        {
            activ = true; // the vqbit is in a superposition state
        }

        protected void setInactive()
        {
            activ = false; // the vqubit is in a collapsed state (0 or 1), after being measured
        }

        protected bool isValidTransition(string gate)
        {
        // if more gates are implemented in this class, they have to be added here.
            if ((gate != "ESP") && (gate != "PXG") && (gate != "PZG") && (gate != "JTS") && (gate != "TSC") && (gate != "CNOT"))
            {
                Console.WriteLine("isValidTransition: Invalid gate: " + gate);
                return false;
            }

            bool result = true;
            double st = crtState.getState();
            switch (st)
            {
                case 0.75: // the introduction of (0.75, 0.25) kind of superposition means that not any gate can be apply to any position
                    if ((gate == "ESP") || (gate == "CNOT"))
                        result = false;
                    break;
                case 0.25:
                    if ((gate == "ESP") || (gate == "CNOT"))
                        result = false;
                    break;
                case 0:
                    if (gate == "JTS")  result = false;
                    break;
                case 1:
                    if (gate == "JTS") result = false;
                    break;
                case 0.5:
                    if (gate == "TSC") result = false;
                    break;
                default:
                    break;
            }
            return result;
        }

        //constructor
        public VQbitBasis(short initState, uint size)
        {
            valid = true;
            activ = false;
            regSize = size;
            entangled = null;

            if ((Math.Abs(initState) != 0) && (Math.Abs(initState) != 1))
            {
                Console.WriteLine("VQBitBasis constructor: invallid initial state (Shall be 1 - meaning the ket (0, 1) or 0):  " + initState);
                valid = false;
            }

            if (valid)
            {
                ph = new Phase(regSize);
                if (initState == 1) crtState = new Pair(0, 1);
                if (initState == -1) crtState = new Pair(0, -1);
                if (initState == 0) crtState = new Pair(1, 0);
            }
        }

        public bool isValid()
        {
            return valid;
        }

        //entangles this vqbit to another vqbit. I'm not sure if the concept if correctly implemented
        //if you better understanding of it, please leave a comment
        public bool setEntangled(VQbitBasis source, bool orientation)
        {
            if ((source != null) && (source.isValid()))
            {
                entangled = source;
                valid = false;
                entType = orientation;
                return true;
            }
            return false;
        }

        public void disentangle()
        {
            if (entangled != null)
            {
                entangled = null;
                valid = true;
            }
        }

        public bool isEntangled()
        {
            if (entangled != null) return true;
            return false;
        }

        public void shiftPhase(uint val)
        {
            ph.shiftPhase(val);
        }

        // the implementation for Hadamard gate
        public void setESP()
        {
            if (!isValidTransition("ESP"))
            {
                Console.WriteLine("Invallid transition gate: ESP - a = " + crtState.getA());
                return;
            }
            double val = 1 / Math.Sqrt(2);
            double newa, newb;
            newa = val * crtState.getA() + val * crtState.getB();
            newb = val * crtState.getA() - val * crtState.getB();
            if (crtState.changeVal(newa, newb))
                setActive();
        }

        //the implementation of CNOT gate
        public void CNOT(VQbitBasis control)
        {
            double st = control.getState();
            int thisSignB = this.getSignB();

            if (st == 0) this.setPXG();
            // the target qbit is in equal superposition, then the reference will have the sign shifted
            // while the target will remain unchanged
            // I did not check the behavior on st = 0.75, or 0.25, but so I did not implement this case.
            if (st == 0.5)
                if (thisSignB < 0) control.setPZG();
        }

        //Pauli Z gate
        public void setPZG()
        {
            if (!isValidTransition("PZG"))
            {
                Console.WriteLine("Invallid transition gate: PZG - a = " + crtState.getA());
                return;
            }
            double newa, newb;
            newa = crtState.getA();
            newb = -crtState.getB();
            //    Console.WriteLine("old a: " + crtState.getA() + " new a: " + newa + " old b: " + crtState.getB() + " newb: " + newb);
            if (crtState.changeVal(newa, newb))
                setActive();
        }

        // Pauli X Gate
        public void setPXG()
        {
            if (!isValidTransition("PXG"))
            {
                Console.WriteLine("Invallid transition gate: PXG - a = " + crtState.getA());
                return;
            }
            double newa, newb;
            newa = crtState.getB();
            newb = crtState.getA();
            if (crtState.changeVal(newa, newb))
                setActive();
        }

        // implements the transition from ESP to (3/4, 1/4) and back (for making the commenting task less stressfull, I use the a pow 2 value 
        // then the a, which is 1/sqrt(2) or sqrt(3)/2 and so on. I hope this doesn't create too much confusion
        // because for each (1/2, 1/2) gate there are 2 coresponding (3/4, 1/4) or (1/4, 3/4) gates, I thought it would be would be cool 
        // to use the phase to distinguish between them.
        //This is a composite gate, in the sens that it has 3 disting corresponding matrices, depending on the exact values and signs of the ket (a, b)
        public void setJTS()
        {
            if (!isValidTransition("JTS"))
            {
                Console.WriteLine("Invallid transition gate: JTS - a = " + crtState.getA());
                return;
            }
            double newa = 0, newb = 0;
            double aux1 = Math.Sqrt(6) - Math.Sqrt(2);
            double aux2 = 2 - Math.Sqrt(3);
            double aux3 = Math.Sqrt(6) + Math.Sqrt(2);
            double aux4 = 2 + Math.Sqrt(3);
            short signA = getSignA();
            short signB = getSignB();
            double st = crtState.getState();

            if (((st == 0.25) && (signA == signB)) || ((st == 0.75) && (signA != signB)))
            {
                newa = crtState.getA() * aux4 / aux3 + crtState.getB() / aux3;
                newb = crtState.getA() / aux3 - crtState.getB() * aux4 / aux3;
            }
            if ((st == 0.75) && (signA == signB))
            {
                newa = crtState.getA() * aux2 / aux1 + crtState.getB() / aux1;
                newb = crtState.getA() / aux1 - crtState.getB() * aux2 / aux1;
            }
            if ((st == 0.25) && (signA != signB))
            {
                newa = -crtState.getA() * aux2 / aux1 - crtState.getB() / aux1;
                newb = -crtState.getA() / aux1 + crtState.getB() * aux2 / aux1;
            }
            if (st == 0.5)
            {
                if ( ph.getSign()>= 0) // actually, I'm looking if the base of the phase if odd or even,
                                        // but in the beginning I called it a sign for some reason that I don't remember
                {
                    newa = crtState.getA() * aux4 / aux3 + crtState.getB() / aux3;
                    newb = crtState.getA() / aux3 - crtState.getB() * aux4 / aux3;
                }
                else
                {
                    Console.WriteLine("setJTS: signA = " + signA + " signB = " + signB);
                    if (signA == signB)
                    {
                        newa = crtState.getA() * aux2 / aux1 + crtState.getB() / aux1;
                        newb = crtState.getA() / aux1 - crtState.getB() * aux2 / aux1;
                    }
                    else
                    {
                        newa = -crtState.getA() * aux2 / aux1 - crtState.getB() / aux1;
                        newb = -crtState.getA() / aux1 + crtState.getB() * aux2 / aux1;
                    }
                }
            }
            if (crtState.changeVal(newa, newb))
                setActive();
        }

        //this gate is similar to JTS, moving from (3/4, 1/4) to (1,0) and back (all combinations)
        // similarly, it consists of 3 different matrices
        public void setTSC()
        {
            if (!isValidTransition("TSC"))
            {
                Console.WriteLine("Invallid transition gate: TSC - a = " + crtState.getA());
                return;
            }
            double newa = 0, newb = 0;
            double aux = Math.Sqrt(3) / 2;
            int signA = getSignA();
            int signB = getSignB();
            double state = getState();

            if (((state == 0.75) && (signA == signB)) || ((state == 0.25) && (signA != signB)))
            {
                newa = crtState.getA() * aux + crtState.getB() / 2;
                newb = crtState.getA() / 2 - crtState.getB() * aux;
            }
            if ((state == 0.25) && (signA == signB))
            {
                newa = -crtState.getA() * aux + crtState.getB() / 2;
                newb = crtState.getA() / 2 + crtState.getB() * aux;
            }
            if ((state == 0.75) && (signA != signB))
            {
                newa = crtState.getA() * aux - crtState.getB() / 2;
                newb = -crtState.getA() / 2 - crtState.getB() * aux;
            }
            if ((state == 0) || (state == 1))
            {
                if (ph.getSign() < 0)
                {
                    newa = crtState.getA() * aux + crtState.getB() / 2;
                    newb = crtState.getA() / 2 - crtState.getB() * aux;
                }
                else
                {
                    if (state == 0)
                    {
                        newa = -crtState.getA() * aux + crtState.getB() / 2;
                        newb = crtState.getA() / 2 + crtState.getB() * aux;
                    }
                    else
                    {
                        newa = crtState.getA() * aux - crtState.getB() / 2;
                        newb = -crtState.getA() / 2 - crtState.getB() * aux;
                    }
                }
            }
            if (crtState.changeVal(newa, newb))
                setActive();
        }

        //shows th phase of the vqbit. 
        public void showPhase()
        {
            int[] powTab;
            uint m = ph.returM();
            bool first = true;
            Console.Write("e pow((2*PI*i/2 pow " + m + ")*(");
            powTab = ph.returnNum();
            for (uint i = 0; i < m; i++)
                if (powTab[i] == 0)
                {
                    if (first) first = false;
                    else Console.Write(" + ");
                    Console.Write("2 pow " + i);
                }
            Console.WriteLine("))");
        }

        public virtual short measure() //shall be implemented in the derived class
        {
            return -1;
        }

        public void activate()
        {
            setActive(); //this function is called when the qbit is moved in a superposition state
                         // when it is collapsed after a measurement, the qbit is inactive
        }

        public double getState()
        {
            return crtState.getState();
        }

        public short getSignB()
        {
            double val = crtState.getB();
            if (val < 0) return -1;
            return 1;
        }

        public short getSignA()
        {
            double val = crtState.getA();
            if (val < 0) return -1;
            return 1;
        }
    }

    //this is the actual vqbit
    // the reason for having a derived class, is that over time I several implementations of the "randomnes" function
    // when the vqbit is in ESP and measured, it has to collapse to a 0 or 1 with equal probability
    // I kept this last implementation because it works reasonably well overall, but performed much better then competition
    // when re-using the same registry for multiple calculations and measurements (meaning that after a measurement the registry is 
    // set back to superposition and used again
    // if anyone can implement a better random function, please feel free to contribute.
    // just derive VQBitBasis and make your own implementation

    public class VQbit : VQbitBasis
    {
        private Stopwatch cron = new Stopwatch();
        private long oldTicks;
        private static int noInts = 32; // don't ask why exactly 32, it's an historical artefact. I just saw it's working and didn't change it anymore.
        private int index;
        private Random rand;

        public VQbit(short initS, int i, uint size) : base(initS, size)
        {
            rand = new Random();
            setActive();
            oldTicks = 0;
            Thread.Sleep(noInts + 1);
            index = i;
        }

        override protected void setActive()
        {
            if (activ) return;
            activ = true;
            cron.Start();
        }

        // this is the function that ensure the randomness of the qbit measurement
        override public short measure()
        {
            if (entangled != null)
            {
                if (entType) return entangled.measure();
                else return (short)(1 - entangled.measure());
            }
            else
            {
                double state = crtState.getState();
                if ((state == 0) || (state == 1))
                    return (short)(1 - state);

                cron.Stop();
                setInactive();
                long eticks = cron.ElapsedTicks;
                if (oldTicks == 0)
                    oldTicks = rand.Next(1000000);
                else eticks = (long)((oldTicks * eticks) / (index + 3));
                short mod = (byte)(eticks % noInts);
                oldTicks = eticks;
                short res = -1;
                if (state == 0.5) res = (short)(mod % 2);
                if (state == 0.75)
                {
                    if (mod % 4 == 1) res = 1;
                    else res = 0;
                }
                if (state == 0.25)
                {
                    if (mod % 4 == 1) res = 0;
                    else res = 1;
                }
                crtState.changeVal(1 - res, res);
                return res;
            }
        }
    }

    //we are now in the testing and validation area, only using the concepts defined above
    class ByteVector
    {
        private byte[] val;
        private int size;

        public ByteVector(int sz)
        {
            if (sz < 1)
            {
                Console.WriteLine("ByteVector: Invalid size: " + sz);
                return;
            }
            size = sz;
        }

        public void setVal(byte[] input)
        {
            if (input.Length != size)
            {
                Console.WriteLine("setVal: invalid vector size");
                return;
            }
            val = input;
        }

        //this function checks if a registry multiple measurements generate duplicates.
        public int checkifDuplicate(ByteVector[] source, int refsize)
        {
            bool identical;
            if (refsize < 1)
                return -1;
            for (int i = 0; i < refsize; i++)
            {
                identical = true;
                for (int j = 0; j < size; j++)
                {
                    if (source[i].val[j] != this.val[j])
                    {
                        identical = false;
                        break;
                    }
                }
                if (identical) return i;
            }
            return -1;
        }

        //calculates what's the percentage of 0's in the reasulting measurement set.
        // if it's comming from an equal super position, than it shall be very close to 0.5.
        public double getMedian0()
        {
            double res = 0;
            for (int i = 0; i < size; i++)
                if (val[i] == 0) res++;
            return res / size;
        }

        public byte getVal(int index)
        {
            if ((index < 0) || (index >= size))
            {
                Console.WriteLine("getVal: invalid index " + index);
                return 2;
            }
            return val[index];
        }

        public byte[] getVal()
        {
            return val;
        }
    } // end of ByteVector

    //this class is tesing the ability of the vqbit to generate 0's and 1's when measured, accoring to the superposition
    class TestRandomGen
    {
        private VQbitBasis[] reg1;
        private VQbitBasis[] reg2;

        private ByteVector[] results;

        private int regSize;
        private int noSamples;

        //measures a vqbit registry. The measurement will set the registry to collapsed state
        private byte[] getValueSet(VQbitBasis[] register)
        { 
            byte[] res = new byte[regSize];
            short val = 0;
            for (int idx = 0; idx < regSize; idx++)
            {
                val = register[idx].measure();
                if (val < 0) return null;
                res[idx] = (byte)val;
            }
            return res;
        }

        //writes a byte[] to console
        private void writeToConsole(byte[] input)
        {
            int len = input.Length;
            byte[] aux = new byte[len / 8];

            for (int i = 0; i < len; i++)
            {
                if ((i > 0) && (i % 8 == 0)) Console.Write(" ");
                Console.Write(input[i]);
            }
            Console.WriteLine("");
        }

        public TestRandomGen(int size, int noMeasurements)
        {
            regSize = size;
            noSamples = noMeasurements;

            reg1 = new VQbitBasis[size];
            reg2 = new VQbitBasis[size];
            results = new ByteVector[noMeasurements];
            for (int i = 0; i < noMeasurements; i++)
                results[i] = new ByteVector(size);
        }

        //main function for testing the random generation
        public void runMeasurements(bool spType)
        {
            byte[] result;
            int identical;
            int noDuplicates = 0;
            double[] avgQbit = new double[regSize];
            double avgOverall = 0, minOverall = 1, maxOverall = 0, avg;

            for (int i = 0; i < regSize; i++)
            {
                reg1[i] = new VQbit(0, i, (uint)regSize); //init all with 0
                avgQbit[i] = 0;
            }

            if (spType) // if true, ESP state will be tested, if false, than the registry will be set to (3/4, 1/4)
                for (int i = 0; i < regSize; i++)
                    reg1[i].setESP();
            else
                for (int i = 0; i < regSize; i++)
                {
                    reg1[i].setESP();
                    reg1[i].setJTS();
                }
            for (int i = 0; i <  noSamples; i++)
            {
                result = getValueSet(reg1);
                results[i].setVal(result);
                identical = results[i].checkifDuplicate(results, i);
                if (identical > -1)
                {
                    noDuplicates++;
                    Console.WriteLine("Duplicated data set. Measure " + i + " identical with " + identical);
                    writeToConsole(results[i].getVal());
                }
                avg = results[i].getMedian0();
                if (minOverall > avg) minOverall = avg;
                if (maxOverall < avg) maxOverall = avg;
                avgOverall += avg;
                for (int j = 0; j < regSize; j++)
                {
                    if (spType) reg1[j].setESP();
                    else
                    {
                        if (reg1[j].getState() == 0) reg1[j].setPXG();
                        reg1[j].setTSC();
                    }
                }
            }
            Console.WriteLine("TestRandomGen results: ");
            Console.WriteLine("No of duplicates value sets: " + noDuplicates);
            Console.WriteLine("Average occurence of 0 in all measurement sets: " + avgOverall/noSamples + " for " + noSamples + " measurements");
            Console.WriteLine("Min occurence of 0 in one measurement set: " + minOverall);
            Console.WriteLine("Max occurence of 0 in one measurement set: " + maxOverall);
            Console.WriteLine("Avg occurence of 0 for each VQbit: ");
            for (int i = 0; i < regSize; i++)
            {
                for (int j = 0; j < noSamples; j++)
                    avgQbit[i] += results[j].getVal(i);
                avgQbit[i] = avgQbit[i] / noSamples;
                Console.Write((1-avgQbit[i]) + "  ");
            }
        }

        public void testEntanglement()
        {
            byte[] result;
            bool success;

            for (int i = 0; i < regSize; i++)
            {
                success = false;
                reg1[i] = new VQbit(0, i, (uint)regSize); //init all with 0
                reg2[i] = new VQbit(0, i, (uint)regSize);
                //entangles reg2 to reg1. Half of qbits directly, half reversed
                if (i % 2 == 0)
                    success = reg2[i].setEntangled(reg1[i], true);
                else success = reg2[i].setEntangled(reg1[i], false);
                if (!success) Console.WriteLine("set entangled failed");
            }

            for (int i = 0; i < regSize; i++)
                reg1[i].setESP();
            result = getValueSet(reg1);
            Console.WriteLine("testEntanglement: first value set ");
            writeToConsole(result);

            result = getValueSet(reg2);
            Console.WriteLine("testEntanglement: second value set ");
            //even qbits shall have the same value in reg2 as reg1, the odd ones shall have the opposite value
            writeToConsole(result);
        }

        /*this is not really testing the randomness, but its still link to it
         *for more details about QFT, please check wikipedia or other documentation on internet
         *below if my interpretation of what QFT is:
         *given a set of m, not collapsed qbits
         *first qbit is set to ESP.
         *Then its phase is shifted with the value m-n+1, if the n-th qbit has the value one (for each n between 2 and m)
         *Then second qbit is set to ESP and has its phase shifted with m-n+1 ( for each n between 2 and m-1)
         ....
         *the last qbit will be only set to ESP but has no phase shift
        */
        public VQbitBasis[] QuantumFourierTransform(uint regSize, byte[] initVal)
        {
            if ((regSize < 2) || (regSize > Phase.getMaxSize()))
            {
                Console.WriteLine("QuantumFourierTransform: invalid register size: " + regSize);
                return null;
            }
            VQbitBasis[] reg = new VQbitBasis[regSize];
            for (int i = 0; i < regSize; i++)
            {
                reg[i] = new VQbit(initVal[i], i, regSize);
            }
            Console.WriteLine("QuantumFurierTransform: Resulting Phase ");
            for (int i = 0; i < regSize-1; i++)
            {
                Console.Write("VQBit[" + i + "]: ");
                reg[i].setESP();
                for (int j = i + 1; j < regSize; j++)
                    if (reg[j].getState() == 0)
                        reg[i].shiftPhase((uint)(j - i + 1));
                reg[i].showPhase();
            }
            return reg;
        }

        //this function is shifting the phase of an entire set with a certain value
        //very good to check how a registry previously set to QFT if changing phase by shifting with the same value
        public void registryShiftPhase(VQbitBasis[] reg, uint delta)
        {
            Console.WriteLine("registryShiftPhase: " + delta + ". Resulting Phase: ");
            for (int i = 0; i < reg.Length; i++)
            {
                reg[i].shiftPhase(delta);
                Console.Write("VQBit[" + i + "]: ");
                reg[i].showPhase();
            }
        }
    } // end of TestRandomGen

    //testing various gate transitions
    class TestGateTransitions
    {
        private VQbitBasis[] reg;
        private int regSize;

        //this wouldn't be probably possible with a real quantum registry, but working on a simulation has its advantages
        //this function exposes the internal state of a registry set
        private void exposeVQbits(VQbitBasis[] source, String text)
        {
            int len = source.Length;
            double st;
            short signA, signB;

            Console.WriteLine(text);
            for (int i = 0; i < len; i++)
            {
                if ((i > 0) && (i % 8 == 0)) Console.Write("     ");
                st = source[i].getState();
                signA = source[i].getSignA();
                signB = source[i].getSignB();
                if (st == 0)
                {
                    Console.Write("(0, ");
                    if (signB < 0) Console.Write("-");
                    Console.Write("1)");
                }
                if (st == 1)
                {
                    if (signA < 0) Console.Write("(-");
                    else Console.Write("(");
                    Console.Write("1, 0)");
                }
                if ((st != 0) && (st != 1))
                {
                    if (signA < 0) Console.Write("(-"+st);
                    else Console.Write("("+st);
                    if (signB < 0) Console.Write(", -" + (1-st));
                    else Console.Write(", " + (1-st));
                    Console.Write(")");
                }
            }
            Console.WriteLine("");
        }

        public TestGateTransitions(int size)
        {
            if (size <= 0 )
            {
                Console.WriteLine("TestGateTransitions: invalid register size: " + size);
                return;
            }

            regSize = size;
            reg = new VQbitBasis[size];
        }

        public void runTest()
        {
            for (int i = 0; i < regSize; i++)
                reg[i] = new VQbit(0, i, (uint) regSize);
            exposeVQbits(reg, "Initial state: ");

            for(int i = 0; i < regSize; i++)
            {
                reg[i].setESP();
                if (i % 2 == 1) reg[i].setPZG();
            }
            exposeVQbits(reg, "Set to ESP, every second qbit flip sign (PZG): ");

            for (int i = 0; i < regSize; i++)
                reg[i].setJTS();
            exposeVQbits(reg, "Apply JTS (3/4 - 1/4): ");

            for (int i = 0; i < regSize; i++)
                reg[i].setTSC();
            exposeVQbits(reg, "Apply TSC (3/4 - 1/4) -> 1/0");

            for (int i = 0; i < regSize; i++)
                reg[i].setTSC();
            exposeVQbits(reg, "Apply TSC again and move back to previous state:");

            for (int i = 0; i < regSize; i++)
            {
                reg[i].setTSC();
                reg[i].shiftPhase(1);
                reg[i].setTSC();
            }
            exposeVQbits(reg, "Apply TSC again and move collapsed state, shift phase and then return again to 3/4 - 1/4:");
            // after the above sequence of calls, one can see that the didn't move back into exactly the same state, as a result of the phase shift
            for (int i = 0; i < regSize; i++)
                reg[i].setJTS(); 
            // when returning to ESP, it doesn't matter anymore from each of the 2 (3/4, 1/4) states corresponding to the ESP state, the vqbit is comming
            exposeVQbits(reg, "Back to the initial equal superposition: ");
        }
    } // end of TestGateTransitions

    class Program
    {

        //please feel free to uncomment and execute
        //and extend
        static void Main(string[] args)
        {
            TestRandomGen randGen = new TestRandomGen(32, 1000);
                        randGen.runMeasurements(true);
                        Console.WriteLine("");
                        randGen.runMeasurements(false);
                        Console.WriteLine("");
                        randGen.testEntanglement();
                        Console.WriteLine("");

/*            byte[] source = new byte[32];
            for (int i = 0; i < 32; i++)
                if (i % 3 == 0) source[i] = 1;
                else source[i] = 0;
            VQbitBasis[] reg;
            reg = randGen.QuantumFourierTransform(32, source);
            randGen.registryShiftPhase(reg, 8); */

/*            TestGateTransitions gateTr = new TestGateTransitions(20);
            gateTr.runTest();
            Console.WriteLine(""); */
        }
    }
} // enjoy!