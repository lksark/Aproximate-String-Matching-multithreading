using System;
using System.Collections.Generic;
using System.Threading;

namespace Edit_Distance_w_Dynamic_Programming
{
    class Program
    {
        static int number_of_program_threads;

        static void Main(string[] args)
        {
            number_of_program_threads = Environment.ProcessorCount - 1;

            //string a = "GCTATAC";
            //string b = "GCGTATGC";
            string a = "shakspeare_";
            string b = "shekespr_";
            //string c = "TATTGGCTATACGGTT";
            //string c = "CGGTTCAGTATTGGCTATACGGTTGCCTACTGTAACC";
            string c = "CGGTTCAGTATTGGCTATATTGGACCGGTTGCCTACTGTAACCAGTCCTA";
            string d = "GCGTATGC";

            Console.WriteLine("Approximate Equals example:");
            Console.WriteLine("\nString a = " + a + "\nString b = " + b);
            Console.WriteLine("\nEdit Distance (Number of Differences) = " + editDistance_ApproximateEquals(a, b));

            Console.WriteLine("\nApproximate Contains example:");
            Console.WriteLine("\nString c = " + c + "\nString d = " + d);
            class_ApproximateContains_multithreading ApproximateContains = new class_ApproximateContains_multithreading();
            ApproximateContains.compute_ApproximateContains(2, d, c);
            ApproximateContains.printApproximateContainsResults();

            Console.Read();

        }

        private static int editDistance_ApproximateEquals(string t, string p)
        {
            int[,] D = new int[p.Length + 1, t.Length + 1];

            for (int n = 0; n < p.Length + 1; n++)
                D[n, 0] = n;

            for (int n = 0; n < t.Length + 1; n++)
                D[0, n] = n;

            int distHor, distVer, distDiag;

            for (int i = 1; i < p.Length + 1; i++)
                for (int j = 1; j < t.Length + 1; j++)
                {
                    distHor = D[i, j - 1] + 1;
                    distVer = D[i - 1, j] + 1;

                    if (p[i - 1] == t[j - 1])
                        distDiag = D[i - 1, j - 1];
                    else
                        distDiag = D[i - 1, j - 1] + 1;

                    D[i, j] = Math.Min(Math.Min(distHor, distVer), distDiag);
                }

            //Print the string P, string T differences with '|', '-' display format
            string T_differences_UserDisplay = "";
            string P_differences_UserDisplay = "";
            string print_bar_UserDisplay = "";
            int x = p.Length;
            int y = t.Length;

            while (x > 0)
            {
                if (t[y - 1] == p[x - 1])
                {
                    T_differences_UserDisplay = t[--y] + T_differences_UserDisplay;
                    print_bar_UserDisplay = "|" + print_bar_UserDisplay;
                    P_differences_UserDisplay = p[--x] + P_differences_UserDisplay;
                }
                else if (D[x, y] > D[x, y - 1] && D[x, y - 1] <= D[x - 1, y])
                {
                    T_differences_UserDisplay = t[--y] + T_differences_UserDisplay;
                    print_bar_UserDisplay = " " + print_bar_UserDisplay;
                    P_differences_UserDisplay = "-" + P_differences_UserDisplay;
                }
                else if (D[x, y] > D[x - 1, y] && D[x - 1, y] <= D[x, y - 1])
                {
                    T_differences_UserDisplay = "-" + T_differences_UserDisplay;
                    print_bar_UserDisplay = " " + print_bar_UserDisplay;
                    P_differences_UserDisplay = p[--x] + P_differences_UserDisplay;
                }
                else
                {
                    T_differences_UserDisplay = t[--y] + T_differences_UserDisplay;
                    print_bar_UserDisplay = " " + print_bar_UserDisplay;
                    P_differences_UserDisplay = p[--x] + P_differences_UserDisplay;
                }
            }

            Console.WriteLine("\nString P & T differences printed in display format for user to check:");
            Console.WriteLine("1) \'|\' means both characters matching;");
            Console.WriteLine("2) \'-\' means character insertion;");
            Console.WriteLine("3) Without \'|\' & \'-\' means character substitution.");
            Console.WriteLine("T: " + T_differences_UserDisplay);
            Console.WriteLine("   " + print_bar_UserDisplay);
            Console.WriteLine("P: " + P_differences_UserDisplay);


            return D[p.Length, t.Length];   //return Edit Distance, number of differences - 1
        }

        public class class_ApproximateContains_multithreading
        {
            Thread[] threadsArray;

            List<struct_Result_ApproximateContains>[] Lists_P_in_T_results;

            int[] segmentT_start_index;
            int[] segmentT_end_index;

            struct struct_Result_ApproximateContains
            {
                public int EditDistance;
                public int StrP_lastChar_location_in_StrT;
                public int StrP_startChar_location_in_StrT;
                public string UserDisplay_T_differences;
                public string UserDisplay_P_differences;
                public string UserDisplay_print_bar;

                public struct_Result_ApproximateContains(int _EditDistance, int _StrP_startChar_location_in_StrT, int _StrP_lastChar_location_in_StrT, string _UserDisplay_T_differences, string _UserDisplay_P_differences, string _UserDisplay_print_bar)
                {
                    EditDistance = _EditDistance;
                    StrP_startChar_location_in_StrT = _StrP_startChar_location_in_StrT;
                    StrP_lastChar_location_in_StrT = _StrP_lastChar_location_in_StrT;
                    UserDisplay_T_differences = _UserDisplay_T_differences;
                    UserDisplay_P_differences = _UserDisplay_P_differences;
                    UserDisplay_print_bar = _UserDisplay_print_bar;
                }
            }

            public class_ApproximateContains_multithreading()
            {
                threadsArray = new Thread[number_of_program_threads];

                Lists_P_in_T_results = new List<struct_Result_ApproximateContains>[number_of_program_threads];
                for (int i = 0; i < number_of_program_threads; i++)
                    Lists_P_in_T_results[i] = new List<struct_Result_ApproximateContains>();

                segmentT_start_index = new int[number_of_program_threads];
                segmentT_end_index = new int[number_of_program_threads];
            }

            private static List<struct_Result_ApproximateContains> ApproximateContains(int allowableEditDistance, string P, string T, int segmentT_StartIndex)
            {
                Console.WriteLine("String T = " + T);
                int[,] D = new int[P.Length + 1, T.Length + 1];

                for (int n = 0; n < P.Length + 1; n++)
                    D[n, 0] = n;

                //ignore any offset positions of P in T, because str.Contains
                for (int n = 0; n < T.Length + 1; n++)
                    D[0, n] = 0;

                int distHor, distVer, distDiag;

                for (int i = 1; i < P.Length + 1; i++)
                    for (int j = 1; j < T.Length + 1; j++)
                    {
                        distHor = D[i, j - 1] + 1;
                        distVer = D[i - 1, j] + 1;

                        if (P[i - 1] == T[j - 1])
                            distDiag = D[i - 1, j - 1];
                        else
                            distDiag = D[i - 1, j - 1] + 1;

                        D[i, j] = Math.Min(Math.Min(distHor, distVer), distDiag);
                    }

                List<struct_Result_ApproximateContains> list_Result_ApproximateContains = new List<struct_Result_ApproximateContains>();
                for (int j = P.Length; j < T.Length + 1; j++)
                {
                    if (allowableEditDistance >= D[P.Length, j])
                    {
                        //Print the string P, string T differences with '|', '-' display format
                        string UserDisplay_T_differences = "";
                        string UserDisplay_P_differences = "";
                        string UserDisplay_print_bar = "";
                        int x = P.Length;
                        int y = j;

                        while (x > 0 && y > 0)
                        {
                            if (T[y - 1] == P[x - 1])
                            {
                                UserDisplay_T_differences = T[--y] + UserDisplay_T_differences;
                                UserDisplay_print_bar = "|" + UserDisplay_print_bar;
                                UserDisplay_P_differences = P[--x] + UserDisplay_P_differences;
                            }
                            else if (D[x, y] > D[x, y - 1] && D[x, y - 1] <= D[x - 1, y])
                            {
                                UserDisplay_T_differences = T[--y] + UserDisplay_T_differences;
                                UserDisplay_print_bar = " " + UserDisplay_print_bar;
                                UserDisplay_P_differences = "-" + UserDisplay_P_differences;
                            }
                            else if (D[x, y] > D[x - 1, y] && D[x - 1, y] <= D[x, y - 1])
                            {
                                UserDisplay_T_differences = "-" + UserDisplay_T_differences;
                                UserDisplay_print_bar = " " + UserDisplay_print_bar;
                                UserDisplay_P_differences = P[--x] + UserDisplay_P_differences;
                            }
                            else
                            {
                                UserDisplay_T_differences = T[--y] + UserDisplay_T_differences;
                                UserDisplay_print_bar = " " + UserDisplay_print_bar;
                                UserDisplay_P_differences = P[--x] + UserDisplay_P_differences;
                            }
                        }

                        if (x == 0)
                            list_Result_ApproximateContains.Add(new struct_Result_ApproximateContains(D[P.Length, j], segmentT_StartIndex + y, segmentT_StartIndex + j - 1, UserDisplay_T_differences, UserDisplay_P_differences, UserDisplay_print_bar));
                    }
                }

                return list_Result_ApproximateContains;
            }

            public void compute_ApproximateContains(int allowable_EditDistance, string _a, string _b)
            {
                string P, T;

                //longer string will become 'T'
                if (_a.Length > _b.Length)
                {
                    T = _a;
                    P = _b;
                }
                else
                {
                    T = _b;
                    P = _a;
                }

                //If string T is not significantly longer than string P, then we don't need to run all processor threads
                if (number_of_program_threads > T.Length / P.Length / 2)
                    number_of_program_threads = T.Length / P.Length / 2;
                if (number_of_program_threads == 0)
                    number_of_program_threads = 1;
                Console.WriteLine("number_of_program_threads = " + number_of_program_threads + "\n");

                segmentT_start_index[0] = 0;
                for (int i = 0; i < number_of_program_threads; i++)
                {
                    if (i > 0)
                        segmentT_start_index[i] = segmentT_end_index[i - 1] - P.Length + 2;
                    segmentT_end_index[i] = (int)(T.Length / number_of_program_threads * (i + 1) + P.Length - 1);
                }
                segmentT_end_index[number_of_program_threads - 1] = T.Length - 1;

                for (int i = 0; i < number_of_program_threads; i++)
                {
                    int x = i;
                    threadsArray[i] = new Thread(() => Lists_P_in_T_results[x] = ApproximateContains(allowable_EditDistance, P, T.Substring(segmentT_start_index[x], segmentT_end_index[x] - segmentT_start_index[x] + 1), segmentT_start_index[x]));
                }

                try
                {
                    for (int i = 0; i < number_of_program_threads; i++)
                        threadsArray[i].Start();

                    for (int i = 0; i < number_of_program_threads; i++)
                        threadsArray[i].Join();
                    // Join both threads with no timeout
                    // Run both until done.
                    // threads have finished at this point.
                }
                catch (ThreadStateException e)
                {
                    Console.WriteLine(e);  // Display text of exception
                }
                catch (ThreadInterruptedException e)
                {
                    Console.WriteLine(e);  // This exception means that the thread
                                           // was interrupted during a Wait
                }
            }

            public void printApproximateContainsResults()
            {
                Console.WriteLine("\nString P & T differences printed in display format for user to check:");
                Console.WriteLine("1) \'|\' means both characters matching;");
                Console.WriteLine("2) \'-\' means character insertion;");
                Console.WriteLine("3) Without \'|\' & \'-\' means character substitution.");

                Console.WriteLine("\nString T ApproximateContains string P results:\n");

                for (int i = 0; i < number_of_program_threads; i++)
                {
                    foreach (struct_Result_ApproximateContains P_in_T_result in Lists_P_in_T_results[i])
                    {
                        Console.WriteLine("Edit Distance (Number of Differences) = " + P_in_T_result.EditDistance);
                        Console.WriteLine("Location of str P in str T = between " + P_in_T_result.StrP_startChar_location_in_StrT + "~" + P_in_T_result.StrP_lastChar_location_in_StrT);
                        Console.WriteLine("T: " + P_in_T_result.UserDisplay_T_differences);
                        Console.WriteLine("   " + P_in_T_result.UserDisplay_print_bar);
                        Console.WriteLine("P: " + P_in_T_result.UserDisplay_P_differences + "\n");
                    }
                }
            }
        }
    }
}
