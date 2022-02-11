import PropTypes from "prop-types";

export function Suggestion({ xrefValue }) {
  return (
    <div className="px-8 py-6">
      <div className="break-all">{xrefValue}</div>

      {navigator.clipboard && (
        <div className="mt-4 space-x-2">
          <span className="mr-2">Copy:</span>
          <button
            className="px-4 py-2 border-2 bg-gray-200 rounded-md shadow-sm hover:bg-gray-400 transition-colors"
            onClick={() => navigator.clipboard.writeText(`<xref:${xrefValue}>`)}
          >
            None
          </button>

          <button
            className="px-4 py-2 border-2 bg-gray-200 rounded-md shadow-sm hover:bg-gray-400 transition-colors"
            onClick={() => navigator.clipboard.writeText(`<xref:${xrefValue}?displayProperty=fullName>`)}
          >
            Full Name
          </button>

          <button
            className="px-4 py-2 border-2 bg-gray-200 rounded-md shadow-sm hover:bg-gray-400 transition-colors"
            onClick={() => navigator.clipboard.writeText(`<xref:${xrefValue}?displayProperty=nameWithType>`)}
          >
            Name with Type
          </button>
          <button
            className="px-4 py-2 border-2 bg-gray-200 rounded-md shadow-sm hover:bg-gray-400 transition-colors"
            onClick={() => navigator.clipboard.writeText(`[](xref:${xrefValue})`)}
          >
            Custom Text
          </button>
          <button
            className="px-4 py-2 border-2 bg-gray-200 rounded-md shadow-sm hover:bg-gray-400 transition-colors"
            onClick={() => navigator.clipboard.writeText(`xref:${xrefValue}`)}
          >
            Member Only
          </button>
        </div>
      )}
    </div>
  );
}

Suggestion.propTypes = {
  xrefValue: PropTypes.string.isRequired
};
