export function Suggestion({ xrefValue }) {
  return (
    <div className="px-8 py-6">
      <div className="break-all">{xrefValue}</div>

      {navigator.clipboard && (
        <div className="mt-4 space-x-2">
          <span class="mr-2">Copy:</span>
          <button
            className="border px-4 py-2 border-gray-400 shadow-sm"
            onClick={() => navigator.clipboard.writeText(`<xref:${xrefValue}>`)}
          >
            None
          </button>

          <button
            className="border px-4 py-2 border-gray-400 shadow-sm"
            onClick={() => navigator.clipboard.writeText(`<xref:${xrefValue}?displayProperty=fullName>`)}
          >
            Full Name
          </button>

          <button
            className="border px-4 py-2 border-gray-400 shadow-sm"
            onClick={() => navigator.clipboard.writeText(`<xref:${xrefValue}?displayProperty=nameWithType>`)}
          >
            Name with Type
          </button>
        </div>
      )}
    </div>
  );
}
