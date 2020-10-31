if redis.call("EXISTS", @playerKey) == 1 then
    return nil
end
if redis.call("EXISTS", @serverKey) == 0 then
    return nil
end
local hashEntries = redis.call("HGETALL", @serverKey)
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
redis.call(
    "HSET", @playerKey,
    "UserId", @userId,
    "UserName", @userName,
    "CurrentServerCode", code
)
if isPublic then
    redis.call("ZINCRBY", @publicServersByPlayerCountKey, 1, code)
else
    redis.call("ZINCRBY", @privateServersByPlayerCountKey, 1, code)
end
return code
