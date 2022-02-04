export function Suggestion({ xrefValue }) {
  return (
    <div className="border">
      <div className="px-6 py-4">
        <div className="break-all">{xrefValue}</div>

        {navigator.clipboard && (
          <div className="mt-2 space-x-2">
            <button className="border px-2 py-1 border-gray-400 shadow-sm" onClick={() => navigator.clipboard.writeText(`<xref:${xrefValue}>`)}>
              Copy
            </button>

            <button
              className="border px-2 py-1 border-gray-400 shadow-sm"
              onClick={() => navigator.clipboard.writeText(`<xref:${xrefValue}?displayProperty=fullName>`)}
            >
              Copy Full Name
            </button>

            <button
              className="border px-2 py-1 border-gray-400 shadow-sm"
              onClick={() => navigator.clipboard.writeText(`<xref:${xrefValue}?displayProperty=nameWithType>`)}
            >
              Copy Name with Type
            </button>
          </div>
        )}
      </div>
    </div>
  );
}
