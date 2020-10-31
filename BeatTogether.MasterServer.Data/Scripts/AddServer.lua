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
    "Code", @code,
    "IsPublic", @isPublic,
    "MaximumPlayerCount", @maximumPlayerCount
)
redis.call(
    "HSET", @playerKey,
    "UserId", @hostUserId,
    "UserName", @hostUserName,
    "CurrentServerCode", @code
)
redis.call("HSET", @serversByHostUserIdKey, @hostUserId, @code)
redis.call("ZADD", @serversByPlayerCountKey, 1, @code)
return true
