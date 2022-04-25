#include "pch.h"
#include "framework.h"
#include "MathLibrary.h"
#include "time.h"
#include "math.h"
//#include "mkl.h"

extern "C" _declspec(dllexport)
void VM(int n, float Min_Arg, float Max_Arg, int VMF_num, int* Time_Data, float* Accuracy_Data)
{
    float* Single_Args = new float[n];
    for (int i = 0; i < n; i++)
    {
        Single_Args[i] = Min_Arg + (Max_Arg - Min_Arg) / n * i;
    }

    double* Double_Args = new double[n];
    for (int i = 0; i < n; i++)
    {
        Double_Args[i] = (double)(Min_Arg + (Max_Arg - Min_Arg) / n * i);
    }

    float* HA_Vals_Single = new float[n];
    float* EP_Vals_Single = new float[n];
    double* HA_Vals_Double = new double[n];
    double* EP_Vals_Double = new double[n];

    unsigned int Start_Time;											// врем€ начала вычислений
    unsigned int End_Time;												// врем€ конца вычислений

    switch (VMF_num)													// разбор функций
    {
    case 0: Start_Time = clock();										// случай vmsLn
        //vmsLn(n, Single_Args, HA_Vals_Single, VML_HA);
        End_Time = clock();
        Time_Data[0] = End_Time - Start_Time;

        Start_Time = clock();
        //vmsLn(n, Single_Args, EP_Vals_Single, VML_EP);
        End_Time = clock();
        Time_Data[1] = End_Time - Start_Time;
        break;

    case 1: Start_Time = clock();										// случай vmdLn
            //vmdLn(n, Double_Args, HA_Vals_Double, VML_HA);
        End_Time = clock();
        Time_Data[0] = End_Time - Start_Time;

        Start_Time = clock();
        //vmdLn(n, Double_Args, EP_Vals_Double, VML_EP);
        End_Time = clock();
        Time_Data[1] = End_Time - Start_Time;
        break;

    case 2: Start_Time = clock();										// случай vmsLGamma
            //vmsLGamma(n, Single_Args, HA_Vals_Single, VML_HA);
        End_Time = clock();
        Time_Data[0] = End_Time - Start_Time;

        Start_Time = clock();
        //vmsLGamma(n, Single_Args, EP_Vals_Single, VML_EP);
        End_Time = clock();
        Time_Data[1] = End_Time - Start_Time;
        break;

    case 3: Start_Time = clock();										// случай vmdLGamma
            //vmdLGamma(n, Double_Args, HA_Vals_Double, VML_HA);
        End_Time = clock();
        Time_Data[0] = End_Time - Start_Time;

        Start_Time = clock();
        //vmdLGamma(n, Double_Args, EP_Vals_Double, VML_EP);
        End_Time = clock();
        Time_Data[1] = End_Time - Start_Time;
        break;
    }

    float Max_Sub_Val = 0;
    switch (VMF_num)
    {
    case 1: case 3:
        for (int i = 0; i < n; i++)
        {
            if (fabs((float)(HA_Vals_Double[i] - EP_Vals_Double[i])) > Max_Sub_Val)
            {
                Max_Sub_Val = fabs((float)(HA_Vals_Double[i] - EP_Vals_Double[i]));
                Accuracy_Data[0] = (float)Double_Args[i];
                Accuracy_Data[1] = (float)HA_Vals_Double[i];
                Accuracy_Data[2] = (float)EP_Vals_Double[i];
            }
        }
        break;

    case 0: case 2:
        for (int i = 0; i < n; i++)
        {
            if (fabs(HA_Vals_Single[i] - EP_Vals_Single[i]) > Max_Sub_Val)
            {
                Max_Sub_Val = fabs(HA_Vals_Single[i] - EP_Vals_Single[i]);
                Accuracy_Data[0] = Single_Args[i];
                Accuracy_Data[1] = HA_Vals_Single[i];
                Accuracy_Data[2] = EP_Vals_Single[i];
            }
        }
        break;
    }

    delete[] Single_Args;
    delete[] Double_Args;
    delete[] HA_Vals_Single;
    delete[] EP_Vals_Single;
    delete[] HA_Vals_Double;
    delete[] EP_Vals_Double;
}