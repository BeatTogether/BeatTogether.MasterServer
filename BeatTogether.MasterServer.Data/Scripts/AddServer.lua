if redis.call("EXISTS", @serverKey) == 1 then
    return false
end
redis.call(
    "HSET", @serverKey,
    "HostUserId", @hostUserId,
    "HostUserName", @hostUserName,
    "RemoteEndPoint", @remoteEndPoint,
    "Code", @code,
    "IsPublic", @isPublic,
    "DiscoveryPolicy", @discoveryPolicy,
    "InvitePolicy", @invitePolicy,
    "BeatmapDifficultyMask", @beatmapDifficultyMask,
    "GameplayModifiersMask", @gameplayModifiersMask,
    "SongPackBloomFilterTop", @songPackBloomFilterTop,
    "SongPackBloomFilterBottom", @songPackBloomFilterBottom,
    "CurrentPlayerCount", @currentPlayerCount,
    "MaximumPlayerCount", @maximumPlayerCount,
    "Random", @random,
    "PublicKey", @publicKey
)
redis.call("HSET", @serversByHostUserIdKey, @hostUserId, @secret)
redis.call("HSET", @serversByCodeKey, @code, @secret)
redis.call("ZADD", @serversByPlayerCountKey, @currentPlayerCount, @secret)
return true
