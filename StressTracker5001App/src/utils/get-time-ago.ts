export function getTimeAgo(timestamp: number | Date) {
  const date = new Date(timestamp);
  const now = new Date();
  const timeDifference = now.getTime() - date.getTime();

  const seconds = Math.floor(timeDifference / 1000);
  const minutes = Math.floor(seconds / 60);
  const hours = Math.floor(minutes / 60);
  const days = Math.floor(hours / 24);
  const months = Math.floor(days / 30);
  const years = Math.floor(months / 12);

  if (seconds < 60) {
    return `Just Now`;
  } else if (minutes < 60) {
    return `${minutes} min. ago`;
  } else if (hours < 24) {
    return `${hours} hr. ago`;
  } else if (days < 30) {
    return `${days} day${days === 1 ? "" : "s"} ago`;
  } else if (months < 12) {
    return `${months} mo ago`;
  } else {
    return `${years} yr. ago`;
  }
}
