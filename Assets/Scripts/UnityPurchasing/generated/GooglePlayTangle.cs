// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("ksitpEf+bsB48kUSiDb+vTrTp/UZqygLGSQvIAOvYa/eJCgoKCwpKooRjvvWRe1phYARUAFcZ9j1xTe5fnTD0dOQJjiK/N96OmRT4+w2Y10VldkKYjK72z+Uk8shl3ZmKsNB9ah/0ZaR9uDxqbj13IoudtBttAnl+jaQV6+xbnriOeX3bmW73p47DsXr6KaTTy8JzcLKAiWt9lus1To/hILpVcxUVm8Twub2tCbRmtWigz4Nn+pRMKKrAHi8pkSJmfxIX1222RGrKCYpGasoIyurKCgptbL3QFsc71TsJTD/omIdE7pktKyZ8gF+ELFn6LezuIoaWCBH3GqNVOsueW3scg5LFfROqzL1W/YIPpj0JweO0JwZGgvLxs1nLerYiisqKCko");
        private static int[] order = new int[] { 8,8,6,13,4,11,13,12,10,11,10,13,13,13,14 };
        private static int key = 41;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
