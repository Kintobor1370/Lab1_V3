using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;


namespace ClassLibrary1
{
    //.........................МАТЕМАТИЧЕСКИЕ ФУНКЦИИ ИЗ ПАКЕТА ВЕКТОРОНОЙ МАТЕМАТИКИ INTEL MKL
    public enum VMf
    {
        vmsLn,
        vmdLn,
        vmsLGamma,
        vmdLGamma
    }

    //.........................СЕТКА, УЗЛЫ КТОРОЙ ЯВЛЯЮТСЯ АРГУМЕНТАМИ ФУНКЦИИ
    [Serializable]
    public class VMGrid
    {
        public int Num { get; set; }
        public float[] Scope { get; set; }
        public VMf Func { get; set; }
        public float Step { get { return (Scope[1] - Scope[0]) / (Num - 1); } }

        public VMGrid(int n = 2, float min = 0, float max = 1, VMf f = VMf.vmsLn)
        {
            this.Num = n;
            this.Scope = new float[2];
            this.Scope[0] = min;
            this.Scope[1] = max;
            Func = f;
        }
    }

    //.........................РЕЗУЛЬТАТЫ СРАВНЕНИЯ ВРЕМЕНИ ВЫЧИСЛЕНИЙ В РЕЖИМАХ VML_HA И VML_EP
    [Serializable]
    public struct VMTime
    {
        public VMGrid VMG_Data { get; set; }
        public int HA_Time { get; set; }                                                                  // время вычислений с точностью VML_HA
        public int EP_Time { get; set; }                                                                  // время вычислений с точностью VML_EP

        public VMTime(VMGrid g, int HA, int EP)
        {
            this.VMG_Data = g;
            this.HA_Time = HA;
            this.EP_Time = EP;
        }

        public float EP_by_HA
        {
            get
            {
                float res = 0;
                if (HA_Time != 0) res = EP_Time / HA_Time;
                return res;
            }
        }

        public override string ToString()
        {
            return $"Argument Vector Length: {VMG_Data.Num};" +
                   $"Min: {VMG_Data.Scope[0]}; Max: {VMG_Data.Scope[1]};" +
                   $"Step: {VMG_Data.Step};" +
                   $"VM Fuction: {VMG_Data.Func};" +
                   $"HA calculation time: {HA_Time}; HA calculation time: {EP_Time};" +
                   $"EP time by HA time: {EP_by_HA}";
        }
    }

    //.........................РЕЗУЛЬТАТЫ СРАВНЕНИЯ ТОЧНОСТИ ВЫЧИСЛЕНИЙ В РЕЖИМАХ VML_HA И VML_EP
    [Serializable]
    public struct VMAccuracy
    {
        public VMGrid VMG_Data { get; set; }
        public float Max_arg { get; set; }                                                                // значения аргумента, при котором максимально отличаются значения функии в режимах VML_HA и VML_EP
        public float HA_Val { get; set; }                                                                 // значение функции с аргументом Max_arg в режиме VML_HA
        public float EP_Val { get; set; }                                                                 // значение функции с аргументом Max_arg в режиме VML_EP

        public VMAccuracy(VMGrid g, float arg, float HA, float EP)
        {
            this.VMG_Data = g;
            this.Max_arg = arg;
            this.HA_Val = HA;
            this.EP_Val = EP;
        }

        public float Max_Sub_Val { get { return Math.Abs(EP_Val - HA_Val); } }

        public override string ToString()
        {
            return $"Argument Vector Length: {VMG_Data.Num};" +
                   $"Min: {VMG_Data.Scope[0]}; Max: {VMG_Data.Scope[1]};" +
                   $"Step: {VMG_Data.Step};" +
                   $"VM Fuction: {VMG_Data.Func};" +
                   $"Max subtraction argument: {Max_arg}; HA value: {HA_Val}; EP value: {EP_Val};" +
                   $"Max subtraction value: {Max_Sub_Val}";
        }
    }

    //.........................СОЗДАНИЕ КОЛЛЕКЦИЙ VMTime И VMAccuracy
    [Serializable]
    public class VMBenchmark
    {
        [DllImport("C:\\Users\\User\\Desktop\\prog\\C#\\Sem6\\Lab1_V3\\x64\\Debug\\Dll1.dll")]
        static extern void VM(int n, float Min_Arg, float Max_Arg, int VMF_num, int[] Time_Data, float[] Accuracy_Data);
        public ObservableCollection<VMTime> TimeCollection { get; set; }
        public ObservableCollection<VMAccuracy> AccuracyCollection { get; set; }

        public VMBenchmark()
        {
            TimeCollection = new ObservableCollection<VMTime>();
            AccuracyCollection = new ObservableCollection<VMAccuracy>();
        }

        public void AddVMTime(VMGrid g)
        {
            VMGrid Copy = new VMGrid(g.Num, g.Scope[0], g.Scope[1], g.Func);
            int[] Time_Data = new int[2];
            float[] Accuracy_Data = new float[3];

            VM(Copy.Num, Copy.Scope[0], Copy.Scope[1], (int)Copy.Func, Time_Data, Accuracy_Data);
            var New_VMTime = new VMTime(Copy, Time_Data[0], Time_Data[1]);
            Console.WriteLine(New_VMTime.ToString());
            TimeCollection.Add(New_VMTime);
        }

        public void AddVMAccuracy(VMGrid g)
        {
            VMGrid Copy = new VMGrid(g.Num, g.Scope[0], g.Scope[1], g.Func);
            int[] Time_Data = new int[2];
            float[] Accuracy_Data = new float[3];

            VM(Copy.Num, Copy.Scope[0], Copy.Scope[1], (int)Copy.Func, Time_Data, Accuracy_Data);
            var New_VMAccuracy = new VMAccuracy(Copy, Accuracy_Data[0], Accuracy_Data[1], Accuracy_Data[2]);
            AccuracyCollection.Add(New_VMAccuracy);
        }

        public float EP_by_HA_Max
        {
            get
            {
                float max = 0;
                for (int i = 0; i < TimeCollection.Count; i++)
                { if (max < TimeCollection[i].EP_by_HA) max = TimeCollection[i].EP_by_HA; }
                return max;
            }
        }

        public float EP_by_HA_Min
        {
            get
            {
                try
                {
                    float min = TimeCollection[0].EP_by_HA;
                    for (int i = 1; i < TimeCollection.Count; i++)
                    { if (min > TimeCollection[i].EP_by_HA) min = TimeCollection[i].EP_by_HA; }
                    return min;
                }
                catch
                { return 0; }
            }
        }

        public void Display()
        {
            Console.WriteLine("_________________VMTime Collection:_______________");
            for (int i = 0; i < TimeCollection.Count; i++)
                Console.WriteLine($"Element {i + 1}:\n" + TimeCollection[i].ToString() + "\n");

            Console.WriteLine("_______________VMAccuracy Collection:_____________");
            for (int i = 0; i < AccuracyCollection.Count; i++)
                Console.WriteLine($"Element {i + 1}:\n" + TimeCollection[i].ToString() + "\n");

            Console.WriteLine("__________________________________________________");
        }
    }
}