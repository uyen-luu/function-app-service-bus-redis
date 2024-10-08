using IPS.Grow.Shared.Monads;
using JetBrains.Annotations;

namespace IPS.Grow.Shared.Extensions;
[PublicAPI]
public static class MaybeExtensions
{
    [Pure]
    [NotNull]
    public static Either<TL, TR> ToEither<TL, TR>([NotNull] this Maybe<TR> maybe, [NotNull] TL err)
    {
        ThrowIfNull(maybe, nameof(maybe));
        ThrowIfNull(err, nameof(err));
        return maybe.MatchResult(Either<TL, TR>.Left(err), Either<TL, TR>.Right);
    }
}