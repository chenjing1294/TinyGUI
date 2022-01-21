using System.Threading.Tasks;

namespace TinifyAPI
{
    public static class SourceTaskExtensions
    {
        public static async Task<Source> Preserve(this Task<Source> task, params string[] options)
        {
            var source = await task.ConfigureAwait(false);
            return source.Preserve(options);
        }

        public static async Task<Source> Resize(this Task<Source> task, object options)
        {
            var source = await task.ConfigureAwait(false);
            return source.Resize(options);
        }

        public static async Task<ResultMeta> Store(this Task<Source> task, object options, string mark)
        {
            var source = await task.ConfigureAwait(false);
            return await source.Store(options, mark).ConfigureAwait(false);
        }

        public static async Task<Result> GetResult(this Task<Source> task, string mark)
        {
            var source = await task.ConfigureAwait(false);
            return await source.GetResult(mark).ConfigureAwait(false);
        }

        public static async Task ToFile(this Task<Source> task, string path, string mark)
        {
            var source = await task.ConfigureAwait(false);
            await source.ToFile(path, mark).ConfigureAwait(false);
        }

        public static async Task<byte[]> ToBuffer(this Task<Source> task, string mark)
        {
            var source = await task.ConfigureAwait(false);
            return await source.ToBuffer(mark).ConfigureAwait(false);
        }
    }
}
