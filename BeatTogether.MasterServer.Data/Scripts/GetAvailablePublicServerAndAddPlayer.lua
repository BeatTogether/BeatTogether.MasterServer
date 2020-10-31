if redis.call("EXISTS", @playerKey) == 1 then
    return nil
end
local code = redis.call("ZRANGEBYSCORE", @publicServersByPlayerCountKey, "-inf", "+inf", "WITHSCORES", "LIMIT", 0, 1)[1]
if not code then
    return nil
end
local playerCount = redis.call("ZSCORE", @publicServersByPlayerCountKey)
if playerCount >= 5 then
    return nil
end
redis.call(
    "HSET", @playerKey,
    "UserId", @userId,
    "UserName", @userName,
    "CurrentServerCode", code
)
redis.call("ZINCRBY", @publicServersByPlayerCountKey, 1, code)
return code
