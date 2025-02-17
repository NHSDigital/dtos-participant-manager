interface InsetTextProps {
  text: string;
  date: string;
}

export default function InsetText({ text, date }: InsetTextProps) {
  return (
    <div className="nhsuk-inset-text">
      <span className="nhsuk-u-visually-hidden">Information: </span>
      <p>
        {text} <strong>{date}</strong>
      </p>
    </div>
  );
}
