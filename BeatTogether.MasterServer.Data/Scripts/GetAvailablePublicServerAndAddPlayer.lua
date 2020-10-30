local code = redis.call("ZRANGEBYSCORE", @publicServerCodesKey, "-inf", "+inf", "WITHSCORES", "LIMIT", 0, 1)[1]
if not code then
    return nil
end
local currentPlayerCount = redis.call("ZCARD", @publicServerCodesKey)
if currentPlayerCount >= 5 then
    return nil
end
if redis.call("EXISTS", @playerByUserIdKey) == 1 then
    return nil
end
redis.call("HSET", @playerByUserIdKey, "UserId", @userId, "UserName", @userName, "Code", code)
redis.call("ZINCRBY", @publicServerCodesKey, 1, code)
return redis.call("HGETALL", @serversByCodeKey)
