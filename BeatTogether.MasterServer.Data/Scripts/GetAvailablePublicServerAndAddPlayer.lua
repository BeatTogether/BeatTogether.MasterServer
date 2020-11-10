local secret = redis.call("ZRANGEBYSCORE", @publicServersByPlayerCountKey, "-inf", "+inf", "WITHSCORES", "LIMIT", 0, 1)[1]
if not secret then
    return nil
end
local playerCount = redis.call("ZSCORE", @publicServersByPlayerCountKey)
if playerCount >= 5 then
    return nil
end
redis.call("ZINCRBY", @publicServersByPlayerCountKey, 1, secret)
return secret
