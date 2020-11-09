if redis.call("EXISTS", @serverKey) == 1 then
    return false
end
if redis.call("EXISTS", @playerKey) == 1 then
    return false
end
redis.call(
    "HSET", @serverKey,
    "HostUserId", @hostUserId,
    "HostUserName", @hostUserName,
    "RemoteEndPoint", @remoteEndPoint,
    "Secret", @secret,
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
redis.call(
    "HSET", @playerKey,
    "UserId", @hostUserId,
    "UserName", @hostUserName,
    "CurrentServerCode", @code
)
redis.call("HSET", @serversByHostUserIdKey, @hostUserId, @code)
redis.call("ZADD", @serversByPlayerCountKey, @currentPlayerCount, @code)
return true
