module.exports.resolveEnvironmentVariables = (input) => input.replace(/%([^%]+)%/g, (initalValue ,variableName) => {
  const entry = Object.entries(process.env).find(([key]) => key.toLowerCase() === variableName.toLowerCase());
  return entry ? entry[1] : initalValue;
});

module.exports.sleep = async (delay) => new Promise(resolve => setTimeout(resolve, delay));