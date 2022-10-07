import PropTypes from "prop-types";

export default function Button({ children, onClick }) {
  return (
    <button
      type="button"
      className="w-full rounded-md border-2 bg-gray-200 px-4 py-2 shadow-sm transition-colors hover:bg-gray-400 md:w-auto"
      onClick={onClick}
    >
      {children}
    </button>
  );
}

Button.propTypes = {
  children: PropTypes.node.isRequired,
  onClick: PropTypes.func
};
