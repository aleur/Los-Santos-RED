using RAGENativeUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public interface IPlayerTask
{
    void Dispose();
    void Setup();
    void Start();
    int PaymentAmount { get; set; }
    int RepOnCompletion { get; set; }
    int DebtOnFail { get; set; }
    int RepOnFail { get; set; }
    int DaysToComplete { get; set; }
    string DebugName { get; set; }
}

