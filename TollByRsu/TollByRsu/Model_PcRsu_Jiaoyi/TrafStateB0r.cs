using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TollByRsu.Model_PcRsu_Jiaoyi
{
    public class TrafStateB0r : TrafState
    {
        private KtEtcTraf _ktl;

        public TrafStateB0r(KtEtcTraf ktl)
        {
            _ktl = ktl;
        }



        public override void StateWorker(byte[] bxFrame, int index, int count)
        {
            throw new NotImplementedException();
        }

        public override void StateWorker()
        {
            throw new NotImplementedException();
        }
    }
}
