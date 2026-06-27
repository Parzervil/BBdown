using BBDown.Core.Entity;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BBDown
{
    public class BBDownRunner
    {
        /// <summary>
        /// 执行完整下载流程
        /// </summary>
        public async Task RunAsync(MyOption option, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            await Program.DoWorkAsync(option);
        }

        /// <summary>
        /// 仅解析视频信息，不下载
        /// </summary>
        public async Task<VInfo> ParseAsync(MyOption option, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
                throw new OperationCanceledException();

            var (_, _, _, _, input, _, _, aidOri, _) = Program.SetUpWork(option);
            var (_, vInfo, _) = await Program.GetVideoInfoAsync(option, aidOri, input);
            return vInfo;
        }
    }
}
