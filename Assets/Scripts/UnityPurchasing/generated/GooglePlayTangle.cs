// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("/kzP7P7DyMfkSIZIOcPPz8/Lzs0pJoGHxnRHene13vf5o33y3Pb6hYHtFW+w9muIaYjbeu0Q3hiJ+fmlCfyYRq1g7fd6wpwuQfX30huyrVxE0g9ukoc5RkllekoQGCuKvAPAwACUfCXgf+4chcjfCfDAM9WiKIvXRw8WS5xXveVLg/MPDYHfTjl+ua5J8aFnjMWrlAVAgl3qG9Bz1PXkQ2ajXXwcdixXpc221dnys1LtZEkxKMohPBJFutJ/WYN4LeDnFbUftiNE2RpfNvAKzpl6HvYXVomqZ4EqQ8yd3JvnGyOokiuX3Ws8nrB4C8WwBojOG5AEIq9tXRcqD5RLrWi5VYRMz8HO/kzPxMxMz8/OclbqTx8a/kMc3RxY1r0IB8zNz87P");
        private static int[] order = new int[] { 0,4,12,8,11,12,7,8,9,9,12,13,12,13,14 };
        private static int key = 206;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
