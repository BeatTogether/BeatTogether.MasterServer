local secret = redis.call("HGET", @serversByCodeKey, @code)
if not secret then
    return nil
end
local playerCount = 0;
local isPublic = tonumber(hashEntries["IsPublic"]) == 1
if isPublic then
    playerCount = redis.call("ZSCORE", @publicServersByPlayerCountKey, code)
else
    playerCount = redis.call("ZSCORE", @privateServersByPlayerCountKey, code)
end
if playerCount >= tonumber(hashEntries["MaximumPlayerCount"]) then
    return nil
end
if isPublic then
    redis.call("ZINCRBY", @publicServersByPlayerCountKey, 1, secret)
else
    redis.call("ZINCRBY", @privateServersByPlayerCountKey, 1, secret)
end
return secret
