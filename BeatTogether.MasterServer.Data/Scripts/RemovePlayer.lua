if redis.call("EXISTS", @playerKey) == 0 then
    return false
end
local hashEntries = redis.call("HGETALL", @playerKey)
local code = hashEntries["CurrentServerCode"]
if tonumber(redis.call("ZSCORE", @publicServersByPlayerCountKey, code)) > 0 then
    redis.call("ZINCRBY", @publicServersByPlayerCountKey, -1, code)
else if tonumber(redis.call("ZSCORE", @privateServersByPlayerCountKey, code)) > 0 then
    redis.call("ZINCRBY", @privateServersByPlayerCountKey, -1, code)
end
redis.call("DEL", @playerKey)
return true
